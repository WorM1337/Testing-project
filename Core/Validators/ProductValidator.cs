using Core.Models;
using Core.Models.Enums;
using FluentValidation;

namespace Core.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Название продукта обязательно.")
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.");

        RuleFor(p => p.Photos)
            .NotNull().WithMessage("Фотографии обязательны.")
            .Must(photos => photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.");

        RuleFor(p => p.CaloriesPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Калорийность не может быть отрицательной.");

        RuleFor(p => p.ProteinsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество белков не может быть отрицательным.");

        RuleFor(p => p.FatsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество жиров не может быть отрицательным.");

        RuleFor(p => p.CarbsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество углеводов не может быть отрицательным.");

        RuleFor(p => p.Category)
            .Must(c => c != ProductCategory.None).WithMessage("Категория продукта обязательна.");

        RuleFor(p => p.CookingRequirement)
            .NotEmpty().WithMessage("Необходимость готовки должна быть указана.");
    }
}