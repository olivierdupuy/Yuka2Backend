using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConfigController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Public endpoint for the mobile app to fetch the current configuration.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        var config = await _context.AppConfigs.FirstOrDefaultAsync();
        if (config == null)
            return Ok(new
            {
                maintenanceMode = false,
                minAppVersion = "1.0.0",
                latestAppVersion = "1.0.0",
                scanEnabled = true,
                searchEnabled = true,
                favoritesEnabled = true,
                registrationEnabled = true
            });

        return Ok(new
        {
            maintenanceMode = config.MaintenanceMode,
            maintenanceMessage = config.MaintenanceMessage,
            minAppVersion = config.MinAppVersion,
            latestAppVersion = config.LatestAppVersion,
            forceUpdateMessage = config.ForceUpdateMessage,
            scanEnabled = config.ScanEnabled,
            searchEnabled = config.SearchEnabled,
            favoritesEnabled = config.FavoritesEnabled,
            registrationEnabled = config.RegistrationEnabled
        });
    }
}
