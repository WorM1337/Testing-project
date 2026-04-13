using FluentValidation;
using Testing_project.Dtos.Dish;
using Core.Models.Enums;
using Core.Utils;

namespace Testing_project.Validators;

public class UpdateDishDtoValidator : AbstractValidator<UpdateDishDto>
{
    public UpdateDishDtoValidator()
    {
        RuleFor(d => d.Name)
            .MinimumLength(2).WithMessage("Минимальная длина названия — 2 символа.")
            .Must(name =>
            {
                // Проверяем, что после удаления макросов название будет не короче 2 символов
                if (string.IsNullOrWhiteSpace(name)) return true;
                try
                {
                    var (_, cleanName) = DishCategoryParser.Parse(name);
                    return cleanName.Length >= 2;
                }
                catch
                {
                    return false;
                }
            }).WithMessage("Название блюда слишком короткое после удаления макросов (минимум 2 символа).")
            .When(d => !string.IsNullOrWhiteSpace(d.Name));

        RuleFor(d => d.Photos)
            .Must(photos => photos == null || photos.Count <= 5).WithMessage("Нельзя загрузить более 5 фотографий.")
            .When(d => d.Photos != null);
        
        RuleFor(d => d.Ingredients)
            .NotEmpty().WithMessage("Должен быть хотя бы один ингредиент.")
            .When(d => d.Ingredients != null);

        RuleForEach(d => d.Ingredients).SetValidator(new CreateIngredientDtoValidator());
        
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
    }
}
