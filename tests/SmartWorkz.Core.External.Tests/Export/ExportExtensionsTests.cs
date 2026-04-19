namespace SmartWorkz.Core.External.Tests.Export;

public class ExportExtensionsTests
{
    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
    }

    [Fact]
    public async Task ToExcelAsync_WithValidData_ReturnsSuccessResult()
    {
        var data = new List<TestData>
        {
            new TestData { Id = 1, Name = "Item 1", Amount = 100m },
            new TestData { Id = 2, Name = "Item 2", Amount = 200m }
        };

        var result = await data.ToExcelAsync("TestSheet");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ToExcelAsync_WithCustomOptions_AppliesOptions()
    {
        var data = new List<TestData> { new TestData { Id = 1, Name = "Item", Amount = 100m } };
        var options = new ExcelOptions { HeaderBold = true, AutoColumnWidth = true, FreezePanes = true };

        var result = await data.ToExcelAsync("CustomSheet", options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ToPdfAsync_WithValidData_ReturnsSuccessResult()
    {
        var data = new List<TestData>
        {
            new TestData { Id = 1, Name = "Item 1", Amount = 100m },
            new TestData { Id = 2, Name = "Item 2", Amount = 200m }
        };

        var result = await data.ToPdfAsync("Test Report");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ToPdfAsync_WithCustomOptions_AppliesOptions()
    {
        var data = new List<TestData> { new TestData { Id = 1, Name = "Item", Amount = 100m } };
        var options = new PdfOptions { PageSize = "A4", Orientation = "Portrait", IncludePageNumbers = true };

        var result = await data.ToPdfAsync("Custom Report", options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ToExcelAsync_Dictionary_WithValidData_ReturnsSuccessResult()
    {
        var sheets = new Dictionary<string, IEnumerable<object>>
        {
            { "Sheet1", new List<TestData> { new TestData { Id = 1, Name = "Item 1", Amount = 100m } }.Cast<object>().ToList() },
            { "Sheet2", new List<TestData> { new TestData { Id = 2, Name = "Item 2", Amount = 200m } }.Cast<object>().ToList() }
        };

        var result = await sheets.ToExcelAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ToExcelAsync_WithEmptyData_ReturnsFailureResult()
    {
        var data = new List<TestData>();
        var result = await data.ToExcelAsync("Sheet");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ToPdfAsync_WithEmptyData_ReturnsFailureResult()
    {
        var data = new List<TestData>();
        var result = await data.ToPdfAsync("Report");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ToExcelAsync_WithLargeDataset_ReturnsSuccessResult()
    {
        var data = Enumerable.Range(1, 1000).Select(i => new TestData { Id = i, Name = $"Item {i}", Amount = 100m + i }).ToList();
        var result = await data.ToExcelAsync("LargeDataSet");
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ToPdfAsync_WithLargeDataset_ReturnsSuccessResult()
    {
        var data = Enumerable.Range(1, 1000).Select(i => new TestData { Id = i, Name = $"Item {i}", Amount = 100m + i }).ToList();
        var result = await data.ToPdfAsync("Large Report");
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ToExcelAsync_AndToPdfAsync_BothSucceed()
    {
        var data = new List<TestData>
        {
            new TestData { Id = 1, Name = "Item 1", Amount = 100m },
            new TestData { Id = 2, Name = "Item 2", Amount = 200m }
        };

        var excelResult = await data.ToExcelAsync("Export");
        var pdfResult = await data.ToPdfAsync("Export");

        Assert.True(excelResult.Succeeded);
        Assert.True(pdfResult.Succeeded);
    }
}
