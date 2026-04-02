using FluentValidation;
using Testing_project.Dtos.Dish;
using Core.Models.Enums;

namespace Testing_project.Validators;

public class UpdateDishDtoValidator : AbstractValidator<UpdateDishDto>
{
    public UpdateDishDtoValidator()
    {
        RuleFor(d => d.Id)
            .GreaterThan(0).WithMessage("Некорректный ID блюда.");

        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("Название блюда обязательно.")
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.");

        RuleFor(d => d.Photos)
            .NotNull().WithMessage("Список фотографий не может быть null.")
            .Must(photos =>photos == null || photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.");

        RuleFor(d => d.Category)
            .NotEqual(DishCategory.None).WithMessage("Категория блюда обязательна.");
        
        RuleFor(d => d.Ingredients)
            .NotNull().WithMessage("Список ингредиентов не может быть пустым.")
            .NotEmpty().WithMessage("Должен быть хотя бы один ингредиент.");

        RuleForEach(d => d.Ingredients).SetValidator(new CreateIngredientDtoValidator());
    }
}