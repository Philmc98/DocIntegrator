using FluentValidation;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Validators;

/// <summary>
/// Валидатор для DTO создания документа.
/// Проверяет обязательные поля и корректность значений.
/// </summary>
public class CreateDocumentDtoValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title обязателен")
            .MaximumLength(200).WithMessage("Title не должен превышать 200 символов");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content обязателен");

        // Кастомный валидатор статуса документа (единый список допустимых статусов)
        RuleFor(x => x.Status).MustBeValidDocumentStatus();
    }
}
