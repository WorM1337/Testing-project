using FluentValidation;
using Testing_project.Dtos.Dish;
using Core.Models.Enums;

namespace Testing_project.Validators;

public class CreateDishDtoValidator : AbstractValidator<CreateDishDto>
{
    public CreateDishDtoValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("Название блюда обязательно.")
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.");

        RuleFor(d => d.Photos)
            .Must(photos => photos == null || photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.");

        // Категория не обязательна, так как может быть определена через макрос в названии
        RuleFor(d => d.Category)
            .Must(c => c != DishCategory.None)
            .When(d => d.Name == null || !d.Name.Contains("!"))
            .WithMessage("Категория блюда обязательна, если в названии не указан макрос (!десерт, !первое, и т.д.).");

        // Валидация КБЖУ (если указано)
        RuleFor(d => d.CaloriesPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Калорийность не может быть отрицательной.")
            .When(d => d.CaloriesPerServing.HasValue);
            
        RuleFor(d => d.ProteinsPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Белки не могут быть отрицательными.")
            .When(d => d.ProteinsPerServing.HasValue);
            
        RuleFor(d => d.FatsPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Жиры не могут быть отрицательными.")
            .When(d => d.FatsPerServing.HasValue);
            
        RuleFor(d => d.CarbsPerServing)
            .GreaterThanOrEqualTo(0).WithMessage("Углеводы не могут быть отрицательными.")
            .When(d => d.CarbsPerServing.HasValue);
            
        RuleFor(d => d.ServingSize)
            .GreaterThan(0).WithMessage("Размер порции должен быть больше 0.")
            .When(d => d.ServingSize.HasValue);

        // Валидация вложенной коллекции ингредиентов
        RuleFor(d => d.Ingredients)
            .NotNull().WithMessage("Список ингредиентов не может быть пустым.")
            .NotEmpty().WithMessage("Должен быть хотя бы один ингредиент.")
            .Must(ingredients => ingredients.All(i => i != null))
            .WithMessage("Среди ингредиентов есть пустые значения.");

        // Валидация каждого ингредиента внутри списка
        RuleForEach(d => d.Ingredients).SetValidator(new CreateIngredientDtoValidator());
    }
}
