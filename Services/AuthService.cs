using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Yuka2Back.Data;
using Yuka2Back.DTOs;
using Yuka2Back.Models;

namespace Yuka2Back.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<(AuthResponseDto? Response, string? Error)> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return (null, "Un compte avec cet email existe déjà.");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return (null, "Ce nom d'utilisateur est déjà pris.");

        if (!IsPasswordStrong(dto.Password))
            return (null, "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule et un chiffre.");

        var user = new User
        {
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName?.Trim(),
            LastName = dto.LastName?.Trim(),
            EmailVerificationToken = GenerateSecureToken(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = await GenerateAuthResponse(user, null);
        return (response, null);
    }

    public async Task<(AuthResponseDto? Response, string? Error)> Login(LoginDto dto, string? ipAddress)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());

        if (user == null)
            return (null, "Email ou mot de passe incorrect.");

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remaining = (user.LockoutEnd.Value - DateTime.UtcNow).Minutes;
            return (null, $"Compte verrouillé. Réessayez dans {remaining + 1} minute(s).");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
                await _context.SaveChangesAsync();
                return (null, $"Trop de tentatives. Compte verrouillé pour {LockoutDuration.Minutes} minutes.");
            }

            await _context.SaveChangesAsync();
            var attemptsLeft = MaxFailedAttempts - user.FailedLoginAttempts;
            return (null, $"Email ou mot de passe incorrect. {attemptsLeft} tentative(s) restante(s).");
        }

        // Reset failed attempts on success
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // Cleanup old refresh tokens (keep last 5)
        var oldTokens = user.RefreshTokens
            .Where(t => !t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(5)
            .ToList();
        _context.RemoveRange(oldTokens);

        await _context.SaveChangesAsync();

        var response = await GenerateAuthResponse(user, ipAddress);
        return (response, null);
    }

    public async Task<(AuthResponseDto? Response, string? Error)> RefreshToken(string token, string? ipAddress)
    {
        var refreshToken = await _context.Set<RefreshToken>()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken == null)
            return (null, "Token invalide.");

        if (refreshToken.IsRevoked)
        {
            // Potential token reuse attack - revoke all user tokens
            await RevokeAllUserTokens(refreshToken.UserId, ipAddress, "Tentative de réutilisation de token révoqué");
            return (null, "Token révoqué. Tous vos appareils ont été déconnectés par sécurité.");
        }

        if (refreshToken.IsExpired)
            return (null, "Token expiré. Veuillez vous reconnecter.");

        // Rotate refresh token
        var newRefreshToken = CreateRefreshToken(ipAddress);
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReplacedByToken = newRefreshToken.Token;

        var user = refreshToken.User;
        user.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user);
        var favCount = await _context.FavoriteProducts.CountAsync(f => f.UserId == user.Id);

        return (new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiry = DateTime.UtcNow.Add(AccessTokenLifetime),
            User = MapToProfile(user, favCount)
        }, null);
    }

    public async Task<(bool Success, string? Error)> ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (false, "Utilisateur non trouvé.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return (false, "Mot de passe actuel incorrect.");

        if (!IsPasswordStrong(dto.NewPassword))
            return (false, "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule et un chiffre.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Revoke all refresh tokens (force re-login on all devices)
        await RevokeAllUserTokens(userId, null, "Changement de mot de passe");

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<string?> ForgotPassword(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
        if (user == null) return null; // Don't reveal if user exists

        user.PasswordResetToken = GenerateSecureToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // In production, send email here
        return user.PasswordResetToken;
    }

    public async Task<(bool Success, string? Error)> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == dto.Email.Trim().ToLower() &&
            u.PasswordResetToken == dto.Token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null) return (false, "Lien de réinitialisation invalide ou expiré.");

        if (!IsPasswordStrong(dto.NewPassword))
            return (false, "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule et un chiffre.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        await RevokeAllUserTokens(user.Id, null, "Réinitialisation du mot de passe");
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(UserProfileDto? Profile, string? Error)> UpdateProfile(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (null, "Utilisateur non trouvé.");

        if (dto.Username != null)
        {
            var taken = await _context.Users.AnyAsync(u => u.Username == dto.Username.Trim() && u.Id != userId);
            if (taken) return (null, "Ce nom d'utilisateur est déjà pris.");
            user.Username = dto.Username.Trim();
        }

        if (dto.FirstName != null) user.FirstName = dto.FirstName.Trim();
        if (dto.LastName != null) user.LastName = dto.LastName.Trim();
        if (dto.Phone != null) user.Phone = dto.Phone.Trim();
        if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var favCount = await _context.FavoriteProducts.CountAsync(f => f.UserId == userId);
        return (MapToProfile(user, favCount), null);
    }

    public async Task<(UserProfileDto? Profile, string? Error)> UpdatePreferences(int userId, UpdatePreferencesDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (null, "Utilisateur non trouvé.");

        if (dto.DietType != null) user.DietType = dto.DietType;
        if (dto.Allergies != null) user.Allergies = dto.Allergies;
        if (dto.DietaryGoals != null) user.DietaryGoals = dto.DietaryGoals;
        if (dto.NotificationsEnabled.HasValue) user.NotificationsEnabled = dto.NotificationsEnabled.Value;
        if (dto.DarkModeEnabled.HasValue) user.DarkModeEnabled = dto.DarkModeEnabled.Value;
        if (dto.Language != null) user.Language = dto.Language;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var favCount = await _context.FavoriteProducts.CountAsync(f => f.UserId == userId);
        return (MapToProfile(user, favCount), null);
    }

    public async Task<UserProfileDto?> GetProfile(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;
        var favCount = await _context.FavoriteProducts.CountAsync(f => f.UserId == userId);
        return MapToProfile(user, favCount);
    }

    public async Task<UserStatsDto?> GetStats(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddDays(-30);

        var scans = await _context.ScanHistories
            .Where(s => s.UserId == userId)
            .Include(s => s.Product)
            .ToListAsync();

        var nutriDist = scans
            .Where(s => s.Product.NutriScore != null)
            .GroupBy(s => s.Product.NutriScore!)
            .ToDictionary(g => g.Key, g => g.Count());

        return new UserStatsDto
        {
            TotalScans = user.TotalScans,
            TotalFavorites = await _context.FavoriteProducts.CountAsync(f => f.UserId == userId),
            TotalProducts = scans.Select(s => s.ProductId).Distinct().Count(),
            ScansThisWeek = scans.Count(s => s.ScannedAt >= weekAgo),
            ScansThisMonth = scans.Count(s => s.ScannedAt >= monthAgo),
            NutriScoreDistribution = nutriDist,
            AverageHealthScore = scans.Where(s => s.Product.HealthScore.HasValue)
                .Select(s => (double)s.Product.HealthScore!.Value)
                .DefaultIfEmpty(0)
                .Average(),
            MemberSince = user.CreatedAt,
        };
    }

    public async Task<(bool Success, string? Error)> DeleteAccount(int userId, string password, string? ipAddress)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ScanHistories)
            .Include(u => u.FavoriteProducts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return (false, "Utilisateur non trouvé.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Mot de passe incorrect.");

        _context.RemoveRange(user.RefreshTokens);
        _context.RemoveRange(user.ScanHistories);
        _context.RemoveRange(user.FavoriteProducts);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<string?> Logout(int userId, string refreshToken, string? ipAddress, bool keepBiometric = false)
    {
        var token = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId);

        if (token != null && token.IsActive)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.RevokeReason = "Déconnexion";
        }

        string? biometricToken = null;
        if (keepBiometric)
        {
            var user = await _context.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                var bioRefresh = new RefreshToken
                {
                    Token = GenerateSecureToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(365),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
                user.RefreshTokens.Add(bioRefresh);
                biometricToken = bioRefresh.Token;
            }
        }

        await _context.SaveChangesAsync();
        return biometricToken;
    }

    public async Task LogoutAll(int userId, string? ipAddress)
    {
        await RevokeAllUserTokens(userId, ipAddress, "Déconnexion de tous les appareils");
    }

    // --- Helpers ---

    private async Task<AuthResponseDto> GenerateAuthResponse(User user, string? ipAddress)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(ipAddress);

        user.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var favCount = await _context.FavoriteProducts.CountAsync(f => f.UserId == user.Id);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiry = DateTime.UtcNow.Add(AccessTokenLifetime),
            User = MapToProfile(user, favCount)
        };
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("firstName", user.FirstName ?? ""),
            new Claim("lastName", user.LastName ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(AccessTokenLifetime),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static RefreshToken CreateRefreshToken(string? ipAddress)
    {
        return new RefreshToken
        {
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    private async Task RevokeAllUserTokens(int userId, string? ipAddress, string reason)
    {
        var tokens = await _context.Set<RefreshToken>()
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.RevokeReason = reason;
        }
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static bool IsPasswordStrong(string password)
    {
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit);
    }

    private static UserProfileDto MapToProfile(User user, int favCount) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        AvatarUrl = user.AvatarUrl,
        Phone = user.Phone,
        DateOfBirth = user.DateOfBirth,
        DietType = user.DietType,
        Allergies = user.Allergies,
        DietaryGoals = user.DietaryGoals,
        NotificationsEnabled = user.NotificationsEnabled,
        DarkModeEnabled = user.DarkModeEnabled,
        Language = user.Language,
        IsEmailVerified = user.IsEmailVerified,
        TotalScans = user.TotalScans,
        TotalFavorites = favCount,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
    };
}
