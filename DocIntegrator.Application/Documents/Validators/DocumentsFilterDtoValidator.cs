using FluentValidation;
using DocIntegrator.Application.Documents.Filters;

public class DocumentsFilterDtoValidator : AbstractValidator<DocumentsFilterDto>
{
    // Допустимые поля сортировки (что в сущности/DTO)
    private static readonly string[] AllowedSortFields = { "CreatedAt", "Title", "Status" };
    private static readonly string[] AllowedSortDirs = { "Asc", "Desc" };

    public DocumentsFilterDtoValidator()
    {
        // Пагинация
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Страница должна быть больше 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Размер страницы должен быть от 1 до 100.");

        // Сортировка
        RuleFor(x => x.SortBy)
            .NotEmpty()
            .Must(f => AllowedSortFields
                .Any(allowed => string.Equals(allowed, f, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"SortBy должен быть один из: {string.Join(", ", AllowedSortFields)}");

        RuleFor(x => x.SortDir)
            .NotEmpty()
            .Must(d => string.Equals(d, "Asc", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(d, "Desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDir должен быть Asc или Desc");

        // Диапазон дат (если один задан, проверяем корректность)
        When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.CreatedFrom!.Value <= x.CreatedTo!.Value)
                .WithMessage("CreatedFrom должен быть меньше или равен CreatedTo.");
        });

        // Поиск по заголовку — защитимся от слишком коротких/длинных строк (опционально)
        RuleFor(x => x.TitleContains)
            .MaximumLength(200)
            .WithMessage("TitleContains должен содержать не более 200 символов.");
    }
}
