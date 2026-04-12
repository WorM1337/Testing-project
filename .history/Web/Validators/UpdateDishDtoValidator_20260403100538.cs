using FluentValidation;
using Testing_project.Dtos.Dish;
using Core.Models.Enums;

namespace Testing_project.Validators;

public class UpdateDishDtoValidator : AbstractValidator<UpdateDishDto>
{
    public UpdateDishDtoValidator()
    {
        RuleFor(d => d.Name)
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.")
            .When(d => !string.IsNullOrWhiteSpace(d.Name));

        RuleFor(d => d.Photos)
            .Must(photos =>photos == null || photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.")
            .When(d => d.Photos != null);
        
        RuleFor(d => d.Ingredients)
            .NotEmpty().WithMessage("Должен быть хотя бы один ингредиент.")
            .When(d => d.Ingredients != null);

        RuleForEach(d => d.Ingredients).SetValidator(new CreateIngredientDtoValidator());
    }
}