using Core.Models;
using FluentValidation;

namespace Data.Validators;

public class IngredientValidator : AbstractValidator<Ingredient>
{
    public IngredientValidator()
    {
        RuleFor(i => i.ProductId)
            .NotEmpty().WithMessage("Идентификатор продукта обязателен.");

        RuleFor(i => i.DishId)
            .NotEmpty().WithMessage("Идентификатор блюда обязателен.");

        RuleFor(i => i.AmountInGrams)
            .GreaterThan(0).WithMessage("Количество продукта должно быть больше нуля.");
    }
}