using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuka2Back.Data;
using Yuka2Back.DTOs;
using Yuka2Back.Models;

namespace Yuka2Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ProductReviewSummaryDto>> GetProductReviews(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound(new { message = "Produit non trouvé" });

        var reviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                ProductId = r.ProductId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var summary = new ProductReviewSummaryDto
        {
            ProductId = productId,
            AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
            TotalReviews = reviews.Count,
            RatingDistribution = Enumerable.Range(1, 5).ToDictionary(i => i, i => reviews.Count(r => r.Rating == i)),
            Reviews = reviews
        };

        return Ok(summary);
    }

    [Authorize]
    [HttpPost("product/{productId}")]
    public async Task<ActionResult<ReviewDto>> CreateReview(int productId, [FromBody] CreateReviewDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound(new { message = "Produit non trouvé" });

        var existing = await _context.ProductReviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        if (existing) return BadRequest(new { message = "Vous avez déjà laissé un avis pour ce produit" });

        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest(new { message = "La note doit être entre 1 et 5" });

        var review = new ProductReview
        {
            UserId = userId,
            ProductId = productId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsApproved = true
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return Ok(new ReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            Username = user!.Username,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var review = await _context.ProductReviews.FindAsync(id);
        if (review == null) return NotFound();
        if (review.UserId != userId) return Forbid();

        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Avis supprimé" });
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<List<ReviewDto>>> GetMyReviews()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var reviews = await _context.ProductReviews
            .Where(r => r.UserId == userId)
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                ProductId = r.ProductId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }
}
