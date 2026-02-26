namespace Yuka2Back.DTOs;

public class CreateReviewDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductReviewSummaryDto
{
    public int ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
}

public class AdminReviewListDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public bool IsReported { get; set; }
    public string? ModerationNote { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ModerateReviewDto
{
    public bool IsApproved { get; set; }
    public string? ModerationNote { get; set; }
}

public class ReviewStatsDto
{
    public int TotalReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int PendingReviews { get; set; }
    public int ReportedReviews { get; set; }
    public double AverageRating { get; set; }
    public List<MostReviewedProductDto> MostReviewedProducts { get; set; } = new();
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public List<DailyReviewDto> ReviewsPerDay { get; set; } = new();
}

public class MostReviewedProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
}

public class DailyReviewDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
