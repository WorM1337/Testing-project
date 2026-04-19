using Core.Models;
using Core.Models.Enums;
using Testing_project.Dtos;

namespace Testing_project.Extensions;

public static class ProductExtensions
{
    public static void ApplyUpdate(this Product product, UpdateProductDto dto, ExtraFlag? parsedFlags = null)
    {
        if (dto.Name != null)
            product.Name = dto.Name;

        if (dto.Photos != null)
            product.Photos = dto.Photos;

        if (dto.CaloriesPer100g.HasValue)
            product.CaloriesPer100g = dto.CaloriesPer100g.Value;

        if (dto.ProteinsPer100g.HasValue)
            product.ProteinsPer100g = dto.ProteinsPer100g.Value;

        if (dto.FatsPer100g.HasValue)
            product.FatsPer100g = dto.FatsPer100g.Value;

        if (dto.CarbsPer100g.HasValue)
            product.CarbsPer100g = dto.CarbsPer100g.Value;

        if (dto.Composition != null)
            product.Composition = dto.Composition;

        if (dto.Category.HasValue)
            product.Category = dto.Category.Value;

        if (dto.CookingRequirement.HasValue)
            product.CookingRequirement = dto.CookingRequirement.Value;

        // Apply parsed flags if provided
        if (parsedFlags.HasValue)
            product.Flags = parsedFlags.Value;
    }
}
