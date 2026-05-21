namespace SmartInventory.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; }
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
    public string? CategoryName { get; set; }
    public string? SupplierName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; }
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
}

public class UpdateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; }
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
}
