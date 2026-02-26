using System.ComponentModel.DataAnnotations;

namespace Yuka2Back.Models;

public class ShoppingListItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ShoppingListId { get; set; }

    public int? ProductId { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public bool IsChecked { get; set; } = false;

    // Navigation
    public ShoppingList ShoppingList { get; set; } = null!;
    public Product? Product { get; set; }
}
