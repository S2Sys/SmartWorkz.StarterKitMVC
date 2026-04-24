using Xunit;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Core.External.Export;
using SmartWorkz.Shared;
using SmartWorkz.StarterKitMVC.Public.Endpoints;

namespace SmartWorkz.StarterKitMVC.Tests.Integration.Endpoints;

public class ExportEndpointTests
{
    private readonly Mock<IPdfExporter> _mockPdfExporter;
    private readonly Mock<IExcelExporter> _mockExcelExporter;
    private readonly Mock<ILogger<ExportController>> _mockLogger;
    private readonly ExportController _controller;

    public ExportEndpointTests()
    {
        _mockPdfExporter = new Mock<IPdfExporter>();
        _mockExcelExporter = new Mock<IExcelExporter>();
        _mockLogger = new Mock<ILogger<ExportController>>();
        _controller = new ExportController(_mockPdfExporter.Object, _mockExcelExporter.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Export_WithPdfFormat_ReturnsPdfFile()
    {
        // Arrange
        var testData = new object[] { new { Name = "John", Age = 30 } };
        var request = new ExportRequest { Data = testData, Title = "Test Report", Format = "pdf" };

        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
        var result = Result.Ok(pdfBytes);

        _mockPdfExporter.Setup(x => x.ExportAsync(It.IsAny<object[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.Export(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        var fileResult = Assert.IsType<FileContentResult>(response);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("Test Report.pdf", fileResult.FileDownloadName);
        Assert.NotEmpty(fileResult.FileContents);
    }

    [Fact]
    public async Task Export_WithExcelFormat_ReturnsExcelFile()
    {
        // Arrange
        var testData = new object[] { new { Name = "John", Age = 30 } };
        var request = new ExportRequest { Data = testData, Title = "Test Report", Format = "excel" };

        var excelBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP magic bytes (Excel is ZIP)
        var result = Result.Ok(excelBytes);

        _mockExcelExporter.Setup(x => x.ExportAsync(It.IsAny<object[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.Export(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        var fileResult = Assert.IsType<FileContentResult>(response);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Equal("Test Report.xlsx", fileResult.FileDownloadName);
        Assert.NotEmpty(fileResult.FileContents);
    }

    [Fact]
    public async Task Export_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _controller.Export(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Export_WithMissingFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExportRequest { Data = new object[] { }, Title = "Test", Format = null! };

        // Act
        var response = await _controller.Export(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Export_WithUnsupportedFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExportRequest { Data = new object[] { }, Title = "Test", Format = "csv" };

        // Act
        var response = await _controller.Export(request, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.NotNull(badRequestResult.Value);
    }
}
