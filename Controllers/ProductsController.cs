using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;
using Yuka2Back.DTOs;
using Yuka2Back.Models;
using Yuka2Back.Services;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly OpenFoodFactsService _offService;

    public ProductsController(AppDbContext context, OpenFoodFactsService offService)
    {
        _context = context;
        _offService = offService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductSearchDto>>> GetAll([FromQuery] string? search, [FromQuery] string? category, [FromQuery] string? nutriScore)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Categories != null && p.Categories.Contains(category));

        if (!string.IsNullOrWhiteSpace(nutriScore))
            query = query.Where(p => p.NutriScore == nutriScore.ToUpper());

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Name = p.Name,
                Brand = p.Brand,
                ImageUrl = p.ImageUrl,
                NutriScore = p.NutriScore,
                HealthScore = p.HealthScore,
                Categories = p.Categories
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Recherche hybride : d'abord en base locale, puis sur Open Food Facts
    /// </summary>
    [HttpGet("search-off")]
    public async Task<ActionResult<List<ProductSearchDto>>> SearchOpenFoodFacts([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<ProductSearchDto>());

        // D'abord chercher en base locale
        var localResults = await _context.Products
            .Where(p => p.Name.Contains(query) || (p.Brand != null && p.Brand.Contains(query)))
            .OrderBy(p => p.Name)
            .Take(5)
            .Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Name = p.Name,
                Brand = p.Brand,
                ImageUrl = p.ImageUrl,
                NutriScore = p.NutriScore,
                HealthScore = p.HealthScore,
                Categories = p.Categories
            })
            .ToListAsync();

        // Ensuite chercher sur Open Food Facts
        var offProducts = await _offService.SearchProductsAsync(query, pageSize: 15);

        foreach (var offProduct in offProducts)
        {
            // Éviter les doublons
            if (localResults.Any(l => l.Barcode == offProduct.Barcode))
                continue;

            // Sauvegarder en base si pas déjà présent
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == offProduct.Barcode);
            if (existing == null)
            {
                _context.Products.Add(offProduct);
                await _context.SaveChangesAsync();
                existing = offProduct;
            }

            localResults.Add(new ProductSearchDto
            {
                Id = existing.Id,
                Barcode = existing.Barcode,
                Name = existing.Name,
                Brand = existing.Brand,
                ImageUrl = existing.ImageUrl ?? offProduct.ImageUrl,
                NutriScore = existing.NutriScore,
                HealthScore = existing.HealthScore,
                Categories = existing.Categories
            });
        }

        return Ok(localResults);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Recherche par code-barres : d'abord en base, sinon Open Food Facts
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetByBarcode(string barcode)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (product == null)
        {
            // Chercher sur Open Food Facts
            var offProduct = await _offService.GetProductByBarcodeAsync(barcode);
            if (offProduct == null)
                return NotFound(new { message = "Produit non trouvé" });

            // Sauvegarder en base
            _context.Products.Add(offProduct);
            await _context.SaveChangesAsync();
            product = offProduct;
        }

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Scan : cherche en base puis sur OFF, enregistre l'historique
    /// </summary>
    [Authorize]
    [HttpPost("scan/{barcode}")]
    public async Task<ActionResult<ProductDto>> ScanProduct(string barcode)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (product == null)
        {
            // Chercher sur Open Food Facts
            var offProduct = await _offService.GetProductByBarcodeAsync(barcode);
            if (offProduct == null)
                return NotFound(new { message = "Produit non trouvé sur Open Food Facts" });

            _context.Products.Add(offProduct);
            await _context.SaveChangesAsync();
            product = offProduct;
        }

        var history = new ScanHistory
        {
            UserId = userId,
            ProductId = product.Id,
            ScannedAt = DateTime.UtcNow
        };

        _context.ScanHistories.Add(history);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Scan sans authentification : cherche en base puis sur OFF
    /// </summary>
    [HttpPost("scan-anonymous/{barcode}")]
    public async Task<ActionResult<ProductDto>> ScanAnonymous(string barcode)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (product == null)
        {
            var offProduct = await _offService.GetProductByBarcodeAsync(barcode);
            if (offProduct == null)
                return NotFound(new { message = "Produit non trouvé sur Open Food Facts" });

            _context.Products.Add(offProduct);
            await _context.SaveChangesAsync();
            product = offProduct;
        }

        return Ok(MapToDto(product));
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<ActionResult<List<ScanHistoryDto>>> GetHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var history = await _context.ScanHistories
            .Where(s => s.UserId == userId)
            .Include(s => s.Product)
            .OrderByDescending(s => s.ScannedAt)
            .Take(50)
            .Select(s => new ScanHistoryDto
            {
                Id = s.Id,
                ScannedAt = s.ScannedAt,
                Product = new ProductSearchDto
                {
                    Id = s.Product.Id,
                    Barcode = s.Product.Barcode,
                    Name = s.Product.Name,
                    Brand = s.Product.Brand,
                    ImageUrl = s.Product.ImageUrl,
                    NutriScore = s.Product.NutriScore,
                    HealthScore = s.Product.HealthScore,
                    Categories = s.Product.Categories
                }
            })
            .ToListAsync();

        return Ok(history);
    }

    [Authorize]
    [HttpPost("favorites/{productId}")]
    public async Task<IActionResult> AddFavorite(int productId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var exists = await _context.FavoriteProducts
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId);

        if (exists) return BadRequest(new { message = "Déjà en favoris" });

        _context.FavoriteProducts.Add(new FavoriteProduct
        {
            UserId = userId,
            ProductId = productId
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Ajouté aux favoris" });
    }

    [Authorize]
    [HttpDelete("favorites/{productId}")]
    public async Task<IActionResult> RemoveFavorite(int productId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var fav = await _context.FavoriteProducts
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

        if (fav == null) return NotFound();

        _context.FavoriteProducts.Remove(fav);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Retiré des favoris" });
    }

    [Authorize]
    [HttpGet("favorites")]
    public async Task<ActionResult<List<ProductSearchDto>>> GetFavorites()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var favorites = await _context.FavoriteProducts
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new ProductSearchDto
            {
                Id = f.Product.Id,
                Barcode = f.Product.Barcode,
                Name = f.Product.Name,
                Brand = f.Product.Brand,
                ImageUrl = f.Product.ImageUrl,
                NutriScore = f.Product.NutriScore,
                HealthScore = f.Product.HealthScore,
                Categories = f.Product.Categories
            })
            .ToListAsync();

        return Ok(favorites);
    }

    [Authorize]
    [HttpPost("compare")]
    public async Task<ActionResult<CompareResultDto>> CompareProducts([FromBody] CompareRequestDto dto)
    {
        if (dto.ProductIds.Count < 2 || dto.ProductIds.Count > 4)
            return BadRequest(new { message = "Sélectionnez entre 2 et 4 produits" });

        var products = await _context.Products
            .Where(p => dto.ProductIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != dto.ProductIds.Count)
            return BadRequest(new { message = "Un ou plusieurs produits introuvables" });

        // Log comparison
        int? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null) userId = int.Parse(userIdClaim);

        _context.ProductComparisons.Add(new ProductComparison
        {
            UserId = userId,
            ProductIds = string.Join(",", dto.ProductIds)
        });
        await _context.SaveChangesAsync();

        var productDtos = products.Select(MapToDto).ToList();

        var result = new CompareResultDto
        {
            Products = productDtos,
            BestHealthScoreProductId = products
                .Where(p => p.HealthScore.HasValue)
                .OrderByDescending(p => p.HealthScore)
                .FirstOrDefault()?.Id,
            BestNutriScoreProductId = products
                .Where(p => p.NutriScore != null)
                .OrderBy(p => p.NutriScore)
                .FirstOrDefault()?.Id
        };

        return Ok(result);
    }

    [Authorize]
    [HttpGet("allergen-check/{productId}")]
    public async Task<ActionResult<AllergenCheckResultDto>> CheckAllergens(int productId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Utilisateur non trouvé" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound(new { message = "Produit non trouvé" });

        var userAllergens = string.IsNullOrWhiteSpace(user.Allergies)
            ? new List<string>()
            : user.Allergies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var productAllergens = string.IsNullOrWhiteSpace(product.Allergens)
            ? new List<string>()
            : product.Allergens.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var matched = userAllergens
            .Where(ua => productAllergens.Any(pa => pa.Contains(ua, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matched.Count > 0)
        {
            _context.AllergenAlerts.Add(new AllergenAlert
            {
                UserId = userId,
                ProductId = productId,
                MatchedAllergens = string.Join(",", matched)
            });
            await _context.SaveChangesAsync();
        }

        return Ok(new AllergenCheckResultDto
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductAllergens = productAllergens,
            UserAllergens = userAllergens,
            MatchedAllergens = matched,
            HasAlert = matched.Count > 0
        });
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Barcode = p.Barcode,
        Name = p.Name,
        Brand = p.Brand,
        ImageUrl = p.ImageUrl,
        NutriScore = p.NutriScore,
        NovaGroup = p.NovaGroup,
        HealthScore = p.HealthScore,
        Calories = p.Calories,
        Fat = p.Fat,
        SaturatedFat = p.SaturatedFat,
        Sugars = p.Sugars,
        Salt = p.Salt,
        Fiber = p.Fiber,
        Proteins = p.Proteins,
        Carbohydrates = p.Carbohydrates,
        Ingredients = p.Ingredients,
        Allergens = p.Allergens,
        Categories = p.Categories,
        Quantity = p.Quantity,
        IsOrganic = p.IsOrganic,
        IsPalmOilFree = p.IsPalmOilFree,
        IsVegan = p.IsVegan
    };
}
