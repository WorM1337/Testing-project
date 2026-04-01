using Core.Models;
using Core.Models.Enums;
using FluentValidation;

namespace Data.Validators;

public class DishValidator : AbstractValidator<Dish>
{
    public DishValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("Название блюда обязательно.")
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.");
        RuleFor(d => d.Photos)
            .NotNull().WithMessage("Фотографии обязательны.")
            .Must(photos => photos.Count <= 5)
            .WithMessage("Нельзя загрузить более 5 фотографий.");
        RuleFor(d => d.CaloriesPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Калорийность не может быть отрицательной.");
        RuleFor(d => d.ProteinsPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Белки не могут быть отрицательными.");
        RuleFor(d => d.FatsPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Жиры не могут быть отрицательными.");
        RuleFor(d => d.CarbsPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Углеводы не могут быть отрицательными.");
        RuleFor(d => d.ServingSize)
            .GreaterThan(0).WithMessage("Размер порции должен быть больше нуля.");
        RuleFor(d => d.Category)
            .Must(c => c != DishCategory.None).WithMessage("Категория блюда обязательна.");
    }
}