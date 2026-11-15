using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveProductRequestValidator
    : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU є обов'язковим.")
            .MaximumLength(50)
            .WithMessage("SKU не може перевищувати 50 символів.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Назва товару є обов'язковою.")
            .MaximumLength(150)
            .WithMessage("Назва не може перевищувати 150 символів.");

        RuleFor(x => x.Unit)
            .NotEmpty()
            .WithMessage("Одиниця вимірювання є обов'язковою.")
            .MaximumLength(20)
            .WithMessage("Одиниця вимірювання не може перевищувати 20 символів.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Кількість не може бути від'ємною.");
    }
}
