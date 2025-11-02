using FluentValidation;
using DocIntegrator.Application.Documents.Filters;

public class DocumentsFilterDtoValidator : AbstractValidator<DocumentsFilterDto>
{
    // Допустимые поля сортировки — приведи к тем же именам, что в сущности/DTO.
    private static readonly string[] AllowedSortFields = { "CreatedAt", "Title", "Status" };
    private static readonly string[] AllowedSortDirs = { "Asc", "Desc" };

    public DocumentsFilterDtoValidator()
    {
        // Пагинация
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        // Сортировка
        RuleFor(x => x.SortBy)
            .NotEmpty()
            .Must(f => AllowedSortFields
                .Any(allowed => string.Equals(allowed, f, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}");

        RuleFor(x => x.SortDir)
            .NotEmpty()
            .Must(d => string.Equals(d, "Asc", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(d, "Desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDir must be Asc or Desc");

        // Диапазон дат (если один задан, проверяем корректность)
        When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.CreatedFrom!.Value <= x.CreatedTo!.Value)
                .WithMessage("CreatedFrom must be less than or equal to CreatedTo.");
        });

        // Поиск по заголовку — защитимся от слишком коротких/длинных строк (опционально)
        RuleFor(x => x.TitleContains)
            .MaximumLength(200)
            .WithMessage("TitleContains must be at most 200 characters.");
    }
}
