namespace Yuka2Back.DTOs;

public class AllergenCheckResultDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public List<string> ProductAllergens { get; set; } = new();
    public List<string> UserAllergens { get; set; } = new();
    public List<string> MatchedAllergens { get; set; } = new();
    public bool HasAlert { get; set; }
}

public class AllergenAlertStatsDto
{
    public int TotalAlerts { get; set; }
    public int AlertsToday { get; set; }
    public int AlertsThisWeek { get; set; }
    public Dictionary<string, int> AllergenDistribution { get; set; } = new();
    public List<MostAlertedProductDto> MostAlertedProducts { get; set; } = new();
}

public class MostAlertedProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int AlertCount { get; set; }
}
