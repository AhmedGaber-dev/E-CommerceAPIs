using AutoMapper;
using ECommerce.Application.Features.Cart;
using ECommerce.Application.Features.Categories;
using ECommerce.Application.Features.Orders;
using ECommerce.Application.Features.Products;
using ECommerce.Application.Features.Users;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.Name));

        CreateMap<Category, CategoryDto>();
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryIds, o => o.MapFrom(s => s.ProductCategories.Select(pc => pc.CategoryId)))
            .ForMember(d => d.CategoryNames, o => o.MapFrom(s => s.ProductCategories.Select(pc => pc.Category.Name)));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name));

        CreateMap<Cart, CartDto>();
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Product.Price));
    }
}
