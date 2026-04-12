using FluentValidation;
using Testing_project.Dtos;
using Core.Models.Enums;

namespace Testing_project.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Название продукта обязательно.")
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.");

        RuleFor(p => p.Photos)
            .Must(photos => photos == null || photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.");

        RuleFor(p => p.CaloriesPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Калорийность не может быть отрицательной.");

        RuleFor(p => p.ProteinsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество белков не может быть отрицательным.");

        RuleFor(p => p.FatsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество жиров не может быть отрицательным.");

        RuleFor(p => p.CarbsPer100g)
            .GreaterThanOrEqualTo(0).WithMessage("Количество углеводов не может быть отрицательным.");

        RuleFor(p => p.Category)
            .NotEqual(ProductCategory.None).WithMessage("Категория продукта обязательна.");

        RuleFor(p => p.CookingRequirement)
            .IsInEnum().WithMessage("Требования к готовке обязательны и должны быть корректным значением.");
    }
}
