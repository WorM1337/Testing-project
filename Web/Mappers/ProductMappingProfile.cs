using Core.Models;
using Testing_project.Dtos;

namespace Testing_project.Mappers;

using AutoMapper;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Маппинг из CreateProductDto в Product
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => (DateTime?)null))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<string>()))
            // Маппинг CookingNeeded из DTO в CookingRequirement модели
            .ForMember(dest => dest.CookingRequirement, opt => opt.MapFrom(src => src.CookingRequirement));

        // Маппинг из UpdateProductDto в Product
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<string>()))
            // Маппинг CookingRequirement из DTO в модель
            .ForMember(dest => dest.CookingRequirement, opt => opt.MapFrom(src => src.CookingRequirement));

        // Маппинг из Product в ProductDto
        CreateMap<Product, ProductDto>()
            // Маппинг CookingRequirement модели в CookingNeeded DTO
            .ForMember(dest => dest.CookingRequirement, opt => opt.MapFrom(src => src.CookingRequirement));
    }
}
