using DocIntegrator.Application.Documents.Filters;
using DocIntegrator.Application.Documents.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace DocIntegrator.Tests.Validators;

public class DocumentsFilterDtoValidatorTests
{
    private readonly DocumentsFilterDtoValidator _validator = new();

    [Fact]
    public void ValidFilter_ShouldNotHaveErrors()
    {
        var dto = new DocumentsFilterDto
        {
            Page = 1,
            PageSize = 20,
            SortBy = "CreatedAt",
            SortDir = "Desc"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("CreatedAt", "Desc")]
    [InlineData("Title", "Asc")]
    [InlineData("Status", "Desc")]
    public void AllAllowedSortFields_ShouldNotHaveError(string sortBy, string sortDir)
    {
        var dto = new DocumentsFilterDto { Page = 1, PageSize = 10, SortBy = sortBy, SortDir = sortDir };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        result.ShouldNotHaveValidationErrorFor(x => x.SortDir);
    }

    [Fact]
    public void PageZero_ShouldHaveError()
    {
        var dto = new DocumentsFilterDto { Page = 0, PageSize = 20, SortBy = "CreatedAt", SortDir = "Desc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void PageSizeOutOfRange_ShouldHaveError()
    {
        var dto = new DocumentsFilterDto { Page = 1, PageSize = 101, SortBy = "CreatedAt", SortDir = "Desc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void InvalidSortBy_ShouldHaveError()
    {
        var dto = new DocumentsFilterDto { Page = 1, PageSize = 20, SortBy = "InvalidField", SortDir = "Desc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void InvalidSortDir_ShouldHaveError()
    {
        var dto = new DocumentsFilterDto { Page = 1, PageSize = 20, SortBy = "CreatedAt", SortDir = "Invalid" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SortDir);
    }

    [Fact]
    public void CreatedFromGreaterThanCreatedTo_ShouldHaveError()
    {
        var dto = new DocumentsFilterDto
        {
            Page = 1,
            PageSize = 20,
            SortBy = "CreatedAt",
            SortDir = "Desc",
            CreatedFrom = DateTime.UtcNow,
            CreatedTo = DateTime.UtcNow.AddDays(-1)
        };
        var result = _validator.TestValidate(dto);
        Assert.False(result.IsValid);
    }
}
