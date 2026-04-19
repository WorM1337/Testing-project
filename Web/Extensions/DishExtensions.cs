using AutoMapper;
using Core.Models;
using Core.Models.Enums;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

public static class DishExtensions
{
    public static void ApplyUpdate(this Dish dish, UpdateDishDto dto, IMapper mapper, ExtraFlag? parsedFlags = null)
    {
        if (dto.Name != null)
            dish.Name = dto.Name;

        if (dto.Photos != null)
            dish.Photos = dto.Photos;

        if (dto.Category.HasValue)
            dish.Category = dto.Category.Value;

        // Apply parsed flags if provided (flags are parsed from string in controller)
        if (parsedFlags.HasValue)
            dish.Flags = parsedFlags.Value;

        // КБЖУ - пользовательские переопределения
        // Если пользователь указал значения, они сохраняются и будут использованы сервисом
        // Важно: мы устанавливаем значения только если они были явно переданы в DTO
        if (dto.CaloriesPerServing.HasValue)
            dish.CaloriesPerServing = dto.CaloriesPerServing.Value;

        if (dto.ProteinsPerServing.HasValue)
            dish.ProteinsPerServing = dto.ProteinsPerServing.Value;

        if (dto.FatsPerServing.HasValue)
            dish.FatsPerServing = dto.FatsPerServing.Value;

        if (dto.CarbsPerServing.HasValue)
            dish.CarbsPerServing = dto.CarbsPerServing.Value;

        if (dto.ServingSize.HasValue)
            dish.ServingSize = dto.ServingSize.Value;

        // Логика для ингредиентов:
        // Если клиент прислал список (даже пустой) — мы полностью заменяем состав
        if (dto.Ingredients != null)
        {
            // Маппим DTO ингредиентов в сущности
            var newIngredients = dto.Ingredients.Select(i => mapper.Map<Ingredient>(i)).ToList();
            
            // Очищаем текущий список и добавляем новый
            dish.Ingredients.Clear();
            foreach (var ingredient in newIngredients)
            {
                dish.Ingredients.Add(ingredient);
            }
        }
    }
}
