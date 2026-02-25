using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Yuka2Back.Data;
using Yuka2Back.DTOs;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AdminAuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
    {
        var admin = await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Username == dto.Username && a.IsActive);

        if (admin == null)
            return Unauthorized(new { message = "Invalid credentials." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        admin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateAdminToken(admin);

        return Ok(new AdminAuthResponseDto
        {
            AccessToken = token,
            AccessTokenExpiry = DateTime.UtcNow.AddHours(8),
            Admin = new AdminProfileDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                Role = admin.Role,
                LastLoginAt = admin.LastLoginAt,
                CreatedAt = admin.CreatedAt
            }
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "SuperAdmin" && role != "Admin" && role != "Viewer")
            return Forbid();

        var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (adminIdStr == null || !int.TryParse(adminIdStr, out var adminId))
            return Unauthorized();

        var admin = await _context.AdminUsers.FindAsync(adminId);
        if (admin == null || !admin.IsActive)
            return NotFound(new { message = "Admin not found." });

        return Ok(new AdminProfileDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email,
            Role = admin.Role,
            LastLoginAt = admin.LastLoginAt,
            CreatedAt = admin.CreatedAt
        });
    }

    private string GenerateAdminToken(Models.AdminUser admin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, admin.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
