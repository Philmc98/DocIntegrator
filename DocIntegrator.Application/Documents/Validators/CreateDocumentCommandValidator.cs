using FluentValidation;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Validators;

/// <summary>
/// Валидатор для команды создания документа.
/// MediatR ValidationBehavior получает IValidator&lt;CreateDocumentCommand&gt; — именно этот класс.
/// Правила вынесены сюда напрямую, чтобы имена полей в ошибках были чистыми (Title, Status, …).
/// </summary>
public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
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
