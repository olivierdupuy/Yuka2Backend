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
[Authorize]
public class ShoppingListsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ShoppingListsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ShoppingListDto>>> GetAll()
    {
        var userId = GetUserId();

        var lists = await _context.ShoppingLists
            .Where(sl => sl.UserId == userId && !sl.IsArchived)
            .Include(sl => sl.Items)
            .OrderByDescending(sl => sl.UpdatedAt)
            .Select(sl => new ShoppingListDto
            {
                Id = sl.Id,
                Name = sl.Name,
                IsArchived = sl.IsArchived,
                ItemCount = sl.Items.Count,
                CheckedCount = sl.Items.Count(i => i.IsChecked),
                CreatedAt = sl.CreatedAt,
                UpdatedAt = sl.UpdatedAt
            })
            .ToListAsync();

        return Ok(lists);
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingListDto>> Create([FromBody] CreateShoppingListDto dto)
    {
        var userId = GetUserId();

        var list = new ShoppingList
        {
            UserId = userId,
            Name = dto.Name
        };

        _context.ShoppingLists.Add(list);
        await _context.SaveChangesAsync();

        return Ok(new ShoppingListDto
        {
            Id = list.Id,
            Name = list.Name,
            IsArchived = false,
            ItemCount = 0,
            CheckedCount = 0,
            CreatedAt = list.CreatedAt,
            UpdatedAt = list.UpdatedAt
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShoppingListDetailDto>> GetDetail(int id)
    {
        var userId = GetUserId();

        var list = await _context.ShoppingLists
            .Where(sl => sl.Id == id && sl.UserId == userId)
            .Include(sl => sl.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync();

        if (list == null) return NotFound();

        return Ok(new ShoppingListDetailDto
        {
            Id = list.Id,
            Name = list.Name,
            IsArchived = list.IsArchived,
            Items = list.Items.Select(i => new ShoppingListItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                ProductImageUrl = i.Product?.ImageUrl,
                Name = i.Name,
                Quantity = i.Quantity,
                IsChecked = i.IsChecked
            }).ToList(),
            CreatedAt = list.CreatedAt,
            UpdatedAt = list.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var list = await _context.ShoppingLists
            .Where(sl => sl.Id == id && sl.UserId == userId)
            .FirstOrDefaultAsync();

        if (list == null) return NotFound();

        _context.ShoppingLists.Remove(list);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Liste supprimée" });
    }

    [HttpPost("{listId}/items")]
    public async Task<ActionResult<ShoppingListItemDto>> AddItem(int listId, [FromBody] AddShoppingListItemDto dto)
    {
        var userId = GetUserId();

        var list = await _context.ShoppingLists
            .Where(sl => sl.Id == listId && sl.UserId == userId)
            .FirstOrDefaultAsync();

        if (list == null) return NotFound();

        var item = new ShoppingListItem
        {
            ShoppingListId = listId,
            ProductId = dto.ProductId,
            Name = dto.Name,
            Quantity = dto.Quantity
        };

        _context.ShoppingListItems.Add(item);
        list.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        Product? product = null;
        if (item.ProductId.HasValue)
            product = await _context.Products.FindAsync(item.ProductId);

        return Ok(new ShoppingListItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = product?.Name,
            ProductImageUrl = product?.ImageUrl,
            Name = item.Name,
            Quantity = item.Quantity,
            IsChecked = item.IsChecked
        });
    }

    [HttpPut("{listId}/items/{itemId}")]
    public async Task<ActionResult<ShoppingListItemDto>> UpdateItem(int listId, int itemId, [FromBody] UpdateShoppingListItemDto dto)
    {
        var userId = GetUserId();

        var list = await _context.ShoppingLists
            .Where(sl => sl.Id == listId && sl.UserId == userId)
            .FirstOrDefaultAsync();

        if (list == null) return NotFound();

        var item = await _context.ShoppingListItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ShoppingListId == listId);

        if (item == null) return NotFound();

        if (dto.Name != null) item.Name = dto.Name;
        if (dto.Quantity.HasValue) item.Quantity = dto.Quantity.Value;
        if (dto.IsChecked.HasValue) item.IsChecked = dto.IsChecked.Value;

        list.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ShoppingListItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name,
            ProductImageUrl = item.Product?.ImageUrl,
            Name = item.Name,
            Quantity = item.Quantity,
            IsChecked = item.IsChecked
        });
    }

    [HttpDelete("{listId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(int listId, int itemId)
    {
        var userId = GetUserId();

        var list = await _context.ShoppingLists
            .Where(sl => sl.Id == listId && sl.UserId == userId)
            .FirstOrDefaultAsync();

        if (list == null) return NotFound();

        var item = await _context.ShoppingListItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ShoppingListId == listId);

        if (item == null) return NotFound();

        _context.ShoppingListItems.Remove(item);
        list.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Item supprimé" });
    }
}
