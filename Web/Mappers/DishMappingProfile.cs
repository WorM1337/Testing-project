using Core.Models;
using Core.Models.Enums;
using Testing_project.Dtos.Dish;

namespace Testing_project.Mappers;

using AutoMapper;

public class DishMappingProfile : Profile
{
    public DishMappingProfile()
    {
        // Маппинг для создания блюда
        CreateMap<CreateDishDto, Dish>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CaloriesPerServing, opt => opt.Ignore())
            .ForMember(dest => dest.ProteinsPerServing, opt => opt.Ignore())
            .ForMember(dest => dest.FatsPerServing, opt => opt.Ignore())
            .ForMember(dest => dest.CarbsPerServing, opt => opt.Ignore())
            .ForMember(dest => dest.ServingSize, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<string>()));
        // Маппинг для обновления блюда
        CreateMap<UpdateDishDto, Dish>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<string>()));

        // Маппинг для ответа API
        CreateMap<Dish, DishDto>();
    }
}