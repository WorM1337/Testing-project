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
            // КБЖУ и размер порции маппим явно - они могут быть переопределены пользователем
            .ForMember(dest => dest.CaloriesPerServing, opt => opt.MapFrom(src => src.CaloriesPerServing))
            .ForMember(dest => dest.ProteinsPerServing, opt => opt.MapFrom(src => src.ProteinsPerServing))
            .ForMember(dest => dest.FatsPerServing, opt => opt.MapFrom(src => src.FatsPerServing))
            .ForMember(dest => dest.CarbsPerServing, opt => opt.MapFrom(src => src.CarbsPerServing))
            .ForMember(dest => dest.ServingSize, opt => opt.MapFrom(src => src.ServingSize))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<string>()));
        
        // Маппинг для обновления блюда
        CreateMap<UpdateDishDto, Dish>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos ?? new List<string>()))
            // КБЖУ и размер порции маппим явно - они могут быть переопределены пользователем
            .ForMember(dest => dest.CaloriesPerServing, opt => opt.MapFrom(src => src.CaloriesPerServing))
            .ForMember(dest => dest.ProteinsPerServing, opt => opt.MapFrom(src => src.ProteinsPerServing))
            .ForMember(dest => dest.FatsPerServing, opt => opt.MapFrom(src => src.FatsPerServing))
            .ForMember(dest => dest.CarbsPerServing, opt => opt.MapFrom(src => src.CarbsPerServing))
            .ForMember(dest => dest.ServingSize, opt => opt.MapFrom(src => src.ServingSize));

        // Маппинг для ответа API
        CreateMap<Dish, DishDto>();
    }
}
