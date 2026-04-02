using FluentValidation;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Validators;

public class CreateIngredientDtoValidator : AbstractValidator<CreateIngredientDto>
{
    public CreateIngredientDtoValidator()
    {
        RuleFor(i => i.ProductId)
            .GreaterThan(0).WithMessage("ID продукта должен быть больше нуля.");

        RuleFor(i => i.AmountInGrams)
            .GreaterThan(0).WithMessage("Количество продукта должно быть больше нуля.");
    }
}