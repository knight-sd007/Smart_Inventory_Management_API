namespace SmartInventory.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }

    public Category? Category { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
