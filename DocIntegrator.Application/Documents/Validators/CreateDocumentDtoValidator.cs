using FluentValidation;
using DocIntegrator.Application.Documents.Dtos;

public class CreateDocumentDtoValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title обязателен")
            .MaximumLength(200).WithMessage("Title не должен превышать 200 символов");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content обязателен");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status обязателен")
            .Must(s => s == "Черновик" || s == "Опубликован" || s == "Manual approval")
            .WithMessage("Status должен быть одним из: Черновик, Опубликован, Manual approval");
    }
}
