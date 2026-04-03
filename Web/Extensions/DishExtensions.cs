using AutoMapper;
using Core.Models;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;
// Подключите ваш маппер, если нужно мапить Ingredients, или делайте это вручную
// Обычно для вложенных коллекций лучше использовать IMapper или ручной цикл

public static class DishExtensions
{
    public static void ApplyUpdate(this Dish dish, UpdateDishDto dto, IMapper mapper)
    {
        if (dto.Name != null)
            dish.Name = dto.Name;

        if (dto.Photos != null)
            dish.Photos = dto.Photos;

        if (dto.Category.HasValue)
            dish.Category = dto.Category.Value;

        if (dto.Flags.HasValue)
            dish.Flags = dto.Flags.Value;

        // Логика для ингредиентов:
        // Если клиент прислал список (даже пустой) — мы полностью заменяем состав
        if (dto.Ingredients != null)
        {
            // Маппим DTO ингредиентов в сущности
            var newIngredients = dto.Ingredients.Select(i => mapper.Map<Ingredient>(i)).ToList();
            
            // Очищаем текущий список и добавляем новый
            // Важно: если у вас Ingredients это навигационное свойство EF Core, 
            // лучше не создавать новый список, а обновить существующий, чтобы трекинг не сломался.
            dish.Ingredients.Clear();
            foreach (var ingredient in newIngredients)
            {
                dish.Ingredients.Add(ingredient);
            }
        }
    }
}