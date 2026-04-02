using AutoMapper;
using Core.Models;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Mappers;

public class IngredientMappingProfile : Profile
{
    public IngredientMappingProfile()
    {
        // Маппинг для состава блюда
        CreateMap<CreateIngredientDto, Ingredient>();

        // Маппинг для отображения состава с данными продукта
        CreateMap<Ingredient, IngredientDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.CaloriesPer100g, opt => opt.MapFrom(src => src.Product.CaloriesPer100g))
            .ForMember(dest => dest.ProteinsPer100g, opt => opt.MapFrom(src => src.Product.ProteinsPer100g))
            .ForMember(dest => dest.FatsPer100g, opt => opt.MapFrom(src => src.Product.FatsPer100g))
            .ForMember(dest => dest.CarbsPer100g, opt => opt.MapFrom(src => src.Product.CarbsPer100g));
    }
}