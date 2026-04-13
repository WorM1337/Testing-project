using Core.Models;
using Core.Models.Enums;
using Core.Models.Query;
using Microsoft.EntityFrameworkCore;

namespace Core.Extensions;

public static class DishQueryableExtensions
{
    public static IQueryable<Dish> ApplyFilters(this IQueryable<Dish> query, DishQuery criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            query = query.Where(d => EF.Functions.Like(d.Name.ToLower(), $"%{criteria.Search.ToLower()}%"));
        }

        if (criteria.Category.HasValue && criteria.Category != DishCategory.None)
        {
            query = query.Where(d => d.Category == criteria.Category.Value);
        }

        if (criteria.Flags?.Any() == true)
        {
            query = query.Where(d => criteria.Flags.All(f => d.Flags.HasFlag(f)));
        }

        return query;
    }

    public static IQueryable<Dish> ApplySorting(this IQueryable<Dish> query, DishSortOption sort, bool ascending)
    {
        return sort switch
        {
            DishSortOption.Calories => ascending 
                ? query.OrderBy(d => d.CaloriesPerServing) 
                : query.OrderByDescending(d => d.CaloriesPerServing),
                
            DishSortOption.Proteins => ascending 
                ? query.OrderBy(d => d.ProteinsPerServing) 
                : query.OrderByDescending(d => d.ProteinsPerServing),
                
            DishSortOption.Fats => ascending 
                ? query.OrderBy(d => d.FatsPerServing) 
                : query.OrderByDescending(d => d.FatsPerServing),
                
            DishSortOption.Carbs => ascending 
                ? query.OrderBy(d => d.CarbsPerServing) 
                : query.OrderByDescending(d => d.CarbsPerServing),
                
            DishSortOption.Category => ascending 
                ? query.OrderBy(d => d.Category) 
                : query.OrderByDescending(d => d.Category),
                
            DishSortOption.CreatedAt => ascending 
                ? query.OrderBy(d => d.CreatedAt) 
                : query.OrderByDescending(d => d.CreatedAt),
                
            _ => ascending 
                ? query.OrderBy(d => d.Name) 
                : query.OrderByDescending(d => d.Name)
        };
    }
}
