using FluentValidation;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Validators;

public class UpdateDocumentValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id обязателен");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title обязателен");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content обязателен");
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status обязателен")
            .Must(s => s == "Черновик" || s == "Опубликован" || s == "Manual approval")
            .WithMessage("Status должен быть одним из: Черновик, Опубликован, Manual approval");
    }
}
