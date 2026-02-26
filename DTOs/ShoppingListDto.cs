namespace Yuka2Back.DTOs;

public class CreateShoppingListDto
{
    public string Name { get; set; } = string.Empty;
}

public class ShoppingListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public int ItemCount { get; set; }
    public int CheckedCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ShoppingListDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public List<ShoppingListItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ShoppingListItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImageUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsChecked { get; set; }
}

public class AddShoppingListItemDto
{
    public int? ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class UpdateShoppingListItemDto
{
    public string? Name { get; set; }
    public int? Quantity { get; set; }
    public bool? IsChecked { get; set; }
}

public class ShoppingListStatsDto
{
    public int TotalLists { get; set; }
    public int ActiveLists { get; set; }
    public int ArchivedLists { get; set; }
    public int TotalItems { get; set; }
    public int CheckedItems { get; set; }
    public double AvgItemsPerList { get; set; }
}
