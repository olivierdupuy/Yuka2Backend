namespace Yuka2Back.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public string? NutriScore { get; set; }
    public int? NovaGroup { get; set; }
    public int? HealthScore { get; set; }
    public double? Calories { get; set; }
    public double? Fat { get; set; }
    public double? SaturatedFat { get; set; }
    public double? Sugars { get; set; }
    public double? Salt { get; set; }
    public double? Fiber { get; set; }
    public double? Proteins { get; set; }
    public double? Carbohydrates { get; set; }
    public string? Ingredients { get; set; }
    public string? Allergens { get; set; }
    public string? Categories { get; set; }
    public string? Quantity { get; set; }
    public bool IsOrganic { get; set; }
    public bool IsPalmOilFree { get; set; }
    public bool IsVegan { get; set; }
}

public class ProductSearchDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public string? NutriScore { get; set; }
    public int? HealthScore { get; set; }
    public string? Categories { get; set; }
}

public class ScanHistoryDto
{
    public int Id { get; set; }
    public ProductSearchDto Product { get; set; } = null!;
    public DateTime ScannedAt { get; set; }
}
