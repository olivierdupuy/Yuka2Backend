using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuka2Back.DTOs;
using Yuka2Back.Services;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    private string? GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (result, error) = await _authService.Register(dto);
        if (error != null)
            return BadRequest(new { message = error });
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (result, error) = await _authService.Login(dto, GetIpAddress());
        if (error != null)
            return Unauthorized(new { message = error });
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var (result, error) = await _authService.RefreshToken(dto.RefreshToken, GetIpAddress());
        if (error != null)
            return Unauthorized(new { message = error });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var biometricToken = await _authService.Logout(GetUserId(), dto.RefreshToken, GetIpAddress(), dto.KeepBiometric);
        return Ok(new { message = "Déconnecté avec succès.", biometricToken });
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        await _authService.LogoutAll(GetUserId(), GetIpAddress());
        return Ok(new { message = "Déconnecté de tous les appareils." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var (success, error) = await _authService.ChangePassword(GetUserId(), dto);
        if (!success)
            return BadRequest(new { message = error });
        return Ok(new { message = "Mot de passe modifié avec succès. Veuillez vous reconnecter." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var token = await _authService.ForgotPassword(dto.Email);
        // Always return success to prevent user enumeration
        return Ok(new { message = "Si un compte existe avec cet email, un lien de réinitialisation a été envoyé.", resetToken = token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var (success, error) = await _authService.ResetPassword(dto);
        if (!success)
            return BadRequest(new { message = error });
        return Ok(new { message = "Mot de passe réinitialisé avec succès." });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _authService.GetProfile(GetUserId());
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var (profile, error) = await _authService.UpdateProfile(GetUserId(), dto);
        if (error != null)
            return BadRequest(new { message = error });
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesDto dto)
    {
        var (profile, error) = await _authService.UpdatePreferences(GetUserId(), dto);
        if (error != null)
            return BadRequest(new { message = error });
        return Ok(profile);
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _authService.GetStats(GetUserId());
        if (stats == null) return NotFound();
        return Ok(stats);
    }

    [Authorize]
    [HttpPost("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
    {
        var (success, error) = await _authService.DeleteAccount(GetUserId(), dto.Password, GetIpAddress());
        if (!success)
            return BadRequest(new { message = error });
        return Ok(new { message = "Compte supprimé avec succès." });
    }
}
