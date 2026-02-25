using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Barcode { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Nutri-Score: A, B, C, D, E
    [MaxLength(1)]
    public string? NutriScore { get; set; }

    // Nova Group: 1, 2, 3, 4
    public int? NovaGroup { get; set; }

    // Score sur 100 (comme Yuka)
    public int? HealthScore { get; set; }

    // Valeurs nutritionnelles pour 100g
    public double? Calories { get; set; }
    public double? Fat { get; set; }
    public double? SaturatedFat { get; set; }
    public double? Sugars { get; set; }
    public double? Salt { get; set; }
    public double? Fiber { get; set; }
    public double? Proteins { get; set; }
    public double? Carbohydrates { get; set; }

    [MaxLength(3000)]
    public string? Ingredients { get; set; }

    [MaxLength(500)]
    public string? Allergens { get; set; }

    [MaxLength(200)]
    public string? Categories { get; set; }

    [MaxLength(100)]
    public string? Quantity { get; set; }

    public bool IsOrganic { get; set; }
    public bool IsPalmOilFree { get; set; }
    public bool IsVegan { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ScanHistory> ScanHistories { get; set; } = new List<ScanHistory>();
    public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();
}
