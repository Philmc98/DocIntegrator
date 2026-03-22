using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace DocIntegrator.Tests.Validators;

public class CreateDocumentDtoValidatorTests
{
    private readonly CreateDocumentDtoValidator _validator = new();

    [Fact]
    public void ValidDto_ShouldNotHaveErrors()
    {
        var dto = new CreateDocumentDto
        {
            Title = "Тест",
            Content = "Содержимое",
            Status = "Черновик"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTitle_ShouldHaveError(string? title)
    {
        var dto = new CreateDocumentDto { Title = title!, Content = "x", Status = "Черновик" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void InvalidStatus_ShouldHaveError()
    {
        var dto = new CreateDocumentDto { Title = "x", Content = "x", Status = "Неверный" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void TitleTooLong_ShouldHaveError()
    {
        var dto = new CreateDocumentDto
        {
            Title = new string('a', 201),
            Content = "x",
            Status = "Черновик"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
