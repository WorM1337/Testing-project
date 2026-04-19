using FluentValidation;
using Testing_project.Dtos;
using Core.Models.Enums;

namespace Testing_project.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.")
            .When(p => p.Name != null);

        RuleFor(p => p.Photos)
            .Must(photos => photos == null || photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.")
            .When(p => p.Photos != null);

        RuleFor(p => p.CaloriesPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Калорийность не может быть отрицательной.")
            .When(p => p.CaloriesPer100g != null);

        RuleFor(p => p.ProteinsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество белков не может быть отрицательным.")
            .When(p => p.ProteinsPer100g != null);

        RuleFor(p => p.FatsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество жиров не может быть отрицательным.")
            .When(p => p.FatsPer100g != null);

        RuleFor(p => p.CarbsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество углеводов не может быть отрицательным.")
            .When(p => p.CarbsPer100g != null);
        
        RuleFor(p => p.CookingRequirement)
            .IsInEnum().WithMessage("Требования к готовке обязательны и должны быть корректным значением.");

        // Validation for sum of macronutrients (proteins + fats + carbs <= 100g)
        // Only validate if at least one macronutrient is provided
        RuleFor(p => p)
            .Must(p => 
            {
                var proteins = p.ProteinsPer100g ?? 0;
                var fats = p.FatsPer100g ?? 0;
                var carbs = p.CarbsPer100g ?? 0;
                // If none of the macronutrients are being updated, skip validation
                if (p.ProteinsPer100g == null && p.FatsPer100g == null && p.CarbsPer100g == null)
                    return true;
                return proteins + fats + carbs <= 100;
            })
            .WithMessage(p => 
            {
                var proteins = p.ProteinsPer100g ?? 0;
                var fats = p.FatsPer100g ?? 0;
                var carbs = p.CarbsPer100g ?? 0;
                return $"Сумма белков, жиров и углеводов не может превышать 100 г. Текущая сумма: {(proteins + fats + carbs):F2} г.";
            })
            .When(p => p.ProteinsPer100g == null || p.ProteinsPer100g >= 0)
            .When(p => p.FatsPer100g == null || p.FatsPer100g >= 0)
            .When(p => p.CarbsPer100g == null || p.CarbsPer100g >= 0);
    }
}
