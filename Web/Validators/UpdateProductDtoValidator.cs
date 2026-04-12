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
    }
}
