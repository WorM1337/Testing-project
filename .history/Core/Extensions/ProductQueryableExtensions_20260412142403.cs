using Core.Models;
using Core.Models.Enums;
using Core.Models.Query;
using Microsoft.EntityFrameworkCore;

namespace Core.Extensions;

public static class ProductQueryableExtensions
{
    /// <summary>
    /// Применяет фильтрацию к запросу продуктов
    /// </summary>
    public static IQueryable<Product> ApplyFilters(this IQueryable<Product> query, ProductQuery criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{criteria.Search.ToLower()}%"));
        }

        if (criteria.Category.HasValue)
        {
            query = query.Where(p => p.Category == criteria.Category.Value);
        }

        if (criteria.CookingRequirement.HasValue)
        {
            query = query.Where(p => p.CookingRequirement == criteria.CookingRequirement.Value);
        }

        if (criteria.Flags?.Any() == true)
        {
            // Проверяем, что у продукта есть ВСЕ указанные флаги
            query = query.Where(p => criteria.Flags.All(f => p.Flags.HasFlag(f)));
        }

        return query;
    }

    /// <summary>
    /// Применяет сортировку к запросу продуктов
    /// </summary>
    public static IQueryable<Product> ApplySorting(this IQueryable<Product> query, ProductSortOption sort, bool ascending)
    {
        return sort switch
        {
            ProductSortOption.Calories => ascending 
                ? query.OrderBy(p => p.CaloriesPer100g) 
                : query.OrderByDescending(p => p.CaloriesPer100g),
                
            ProductSortOption.Proteins => ascending 
                ? query.OrderBy(p => p.ProteinsPer100g) 
                : query.OrderByDescending(p => p.ProteinsPer100g),
                
            ProductSortOption.Fats => ascending 
                ? query.OrderBy(p => p.FatsPer100g) 
                : query.OrderByDescending(p => p.FatsPer100g),
                
            ProductSortOption.Carbs => ascending 
                ? query.OrderBy(p => p.CarbsPer100g) 
                : query.OrderByDescending(p => p.CarbsPer100g),
                
            ProductSortOption.CreatedAt => ascending 
                ? query.OrderBy(p => p.CreatedAt) 
                : query.OrderByDescending(p => p.CreatedAt),
                
            _ => ascending 
                ? query.OrderBy(p => p.Name) 
                : query.OrderByDescending(p => p.Name)
        };
    }
}
