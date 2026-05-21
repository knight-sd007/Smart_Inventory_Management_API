namespace SmartInventory.Application.Mappings;

using AutoMapper;
using SmartInventory.Application.DTOs.Category;
using SmartInventory.Application.DTOs.Order;
using SmartInventory.Application.DTOs.Product;
using SmartInventory.Application.DTOs.Supplier;
using SmartInventory.Domain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
        CreateMap<Category, UpdateCategoryDto>().ReverseMap();

        CreateMap<Supplier, SupplierDto>().ReverseMap();
        CreateMap<Supplier, CreateSupplierDto>().ReverseMap();
        CreateMap<Supplier, UpdateSupplierDto>().ReverseMap();

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty));
        CreateMap<Product, CreateProductDto>().ReverseMap();
        CreateMap<Product, UpdateProductDto>().ReverseMap();

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));
        CreateMap<OrderItem, CreateOrderItemDto>().ReverseMap();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Order, CreateOrderDto>().ReverseMap();
        CreateMap<Order, UpdateOrderDto>().ReverseMap();
    }
}
