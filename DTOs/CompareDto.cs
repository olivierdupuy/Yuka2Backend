namespace Yuka2Back.DTOs;

public class CompareRequestDto
{
    public List<int> ProductIds { get; set; } = new();
}

public class CompareResultDto
{
    public List<ProductDto> Products { get; set; } = new();
    public int? BestHealthScoreProductId { get; set; }
    public int? BestNutriScoreProductId { get; set; }
}

public class ComparisonStatsDto
{
    public int TotalComparisons { get; set; }
    public int ComparisonsToday { get; set; }
    public int ComparisonsThisWeek { get; set; }
    public double AvgProductsPerComparison { get; set; }
    public List<MostComparedProductDto> MostComparedProducts { get; set; } = new();
}

public class MostComparedProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CompareCount { get; set; }
}
