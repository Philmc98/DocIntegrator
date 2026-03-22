using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace DocIntegrator.Tests.Validators;

public class UpdateDocumentDtoValidatorTests
{
    private readonly UpdateDocumentDtoValidator _validator = new();

    [Fact]
    public void ValidDto_ShouldNotHaveErrors()
    {
        var dto = new UpdateDocumentDto
        {
            Title = "Обновление",
            Content = "Текст",
            Status = "Опубликован"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Черновик")]
    [InlineData("На согласовании")]
    public void AllAllowedStatuses_ShouldNotHaveError(string status)
    {
        var dto = new UpdateDocumentDto { Title = "x", Content = "x", Status = status };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void InvalidStatus_ShouldHaveError()
    {
        var dto = new UpdateDocumentDto { Title = "x", Content = "x", Status = "Архив" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void EmptyTitle_ShouldHaveError()
    {
        var dto = new UpdateDocumentDto { Title = "", Content = "x", Status = "Черновик" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void EmptyContent_ShouldHaveError()
    {
        var dto = new UpdateDocumentDto { Title = "x", Content = "", Status = "Черновик" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void TitleTooLong_ShouldHaveError()
    {
        var dto = new UpdateDocumentDto
        {
            Title = new string('a', 201),
            Content = "x",
            Status = "Черновик"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
