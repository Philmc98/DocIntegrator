using FluentValidation;

namespace DocIntegrator.Application.Documents.Validators;

/// <summary>
/// Кастомный валидатор статуса документа.
/// Единый список допустимых статусов для создания/обновления — не дублируем в каждом DTO-валидаторе.
/// </summary>
public static class DocumentStatusValidator
{
    /// <summary>
    /// Допустимые статусы документа (бизнес-правило).
    /// </summary>
    public static readonly string[] AllowedStatuses = { "Черновик", "Опубликован", "На согласовании" };

    /// <summary>
    /// Правило для FluentValidation: значение должно быть одним из допустимых статусов.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidDocumentStatus<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Status обязателен")
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage($"Status должен быть одним из: {string.Join(", ", AllowedStatuses)}");
    }
}
