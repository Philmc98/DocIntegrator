using FluentValidation;
using DocIntegrator.Application.Documents.Filters;

public class DocumentsFilterDtoValidator : AbstractValidator<DocumentsFilterDto>
{
    public DocumentsFilterDtoValidator()
    {
        RuleFor(x => x.PrimarySort)
            .Must(v => v is null || new[] { "title", "status", "createdAt" }
                .Contains(v, StringComparer.OrdinalIgnoreCase))
            .WithMessage("primarySort должен быть: title | status | createdAt");

        RuleFor(x => x.PrimarySortOrder)
            .Must(v => new[] { "asc", "desc" }
                .Contains(v!, StringComparer.OrdinalIgnoreCase))
            .WithMessage("primarySortOrder должен быть: asc | desc");

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => x.Page.HasValue);
        RuleFor(x => x.PageSize).GreaterThan(0).When(x => x.PageSize.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.CreatedFrom.HasValue && x.CreatedTo.HasValue && x.CreatedFrom > x.CreatedTo))
            .WithMessage("createdFrom не может быть больше createdTo");
    }
}
