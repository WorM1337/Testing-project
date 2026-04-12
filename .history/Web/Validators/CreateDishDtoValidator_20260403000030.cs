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

        // RuleFor(d => d.Category)
        //     .NotEqual(DishCategory.None).WithMessage("Категория блюда обязательна.");

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