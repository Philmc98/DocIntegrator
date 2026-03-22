using FluentValidation;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Validators;

/// <summary>
/// Валидатор для DTO обновления документа.
/// Проверяет обязательные поля и корректность значений.
/// </summary>
public class UpdateDocumentDtoValidator : AbstractValidator<UpdateDocumentDto>
{
    public UpdateDocumentDtoValidator()
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
