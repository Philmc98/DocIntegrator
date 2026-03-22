using FluentValidation;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Validators;

/// <summary>
/// Валидатор для команды обновления документа.
/// MediatR ValidationBehavior получает IValidator&lt;UpdateDocumentCommand&gt; — именно этот класс.
/// </summary>
public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(x => x.Document.Title)
            .NotEmpty().WithMessage("Title обязателен")
            .MaximumLength(200).WithMessage("Title не должен превышать 200 символов")
            .OverridePropertyName("Title");

        RuleFor(x => x.Document.Content)
            .NotEmpty().WithMessage("Content обязателен")
            .OverridePropertyName("Content");

        RuleFor(x => x.Document.Status)
            .MustBeValidDocumentStatus()
            .OverridePropertyName("Status");
    }
}
