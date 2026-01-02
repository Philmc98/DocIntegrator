using FluentValidation;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Validators;

/// <summary>
/// Валидатор для DTO создания документа.
/// Проверяет обязательные поля и корректность значений.
/// </summary>
public class CreateDocumentDtoValidator : AbstractValidator<CreateDocumentDto>
{
    // Допустимые статусы
    private static readonly string[] AllowedStatuses = { "Черновик", "Опубликован", "На согласовании" };

    public CreateDocumentDtoValidator()
    {
        // Title: обязателен, максимум 200 символов
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title обязателен")
            .MaximumLength(200).WithMessage("Title не должен превышать 200 символов");

        // Content: обязателен
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content обязателен");

        // Status: обязателен и должен быть одним из допустимых значений
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status обязателен")
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage($"Status должен быть одним из: {string.Join(", ", AllowedStatuses)}");
    }
}
