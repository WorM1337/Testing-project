using FluentValidation;
using Testing_project.Dtos;
using Core.Models.Enums;

namespace Testing_project.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0).WithMessage("Некорректный ID продукта.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Название продукта обязательно.")
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.");

        RuleFor(p => p.Photos)
            .NotNull().WithMessage("Список фотографий не может быть null.")
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
            
        RuleFor(p => p.CookingNeeded)
            .IsInEnum().WithMessage("Некорректное значение необходимости готовки.");
    }
}