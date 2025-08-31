using bookmarkr.ServiceAgent;
using Moq;
using Moq.Contrib.HttpClient;
using System.Net;

namespace bookmarkr.Tests.ServiceAgent;

[TestFixture]
public class BookmarkrLookupServiceAgentTests
{
    private Mock<HttpMessageHandler> _mockHandler;
    private IHttpClientFactory _factory;
    private BookmarkrLookupServiceAgent _serviceAgent;

    [SetUp]
    public void SetUp()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _factory = _mockHandler.CreateClientFactory();
        _serviceAgent = new BookmarkrLookupServiceAgent(_factory);
    }

    [Test]
    public void Constructor_WithValidHttpClientFactory_ShouldCreateInstance()
    {
        // Arrange & Act
        var serviceAgent = new BookmarkrLookupServiceAgent(_factory);

        // Assert
        Assert.That(serviceAgent, Is.Not.Null);
        Assert.That(serviceAgent, Is.InstanceOf<IBookmarkrLookupServiceAgent>());
    }

    [Test]
    public async Task GetBookmarkTitle_WithValidHtmlTitle_ShouldReturnTitle()
    {
        // Arrange
        const string expectedTitle = "Test Page Title";
        const string htmlContent = $"<html><head><title>{expectedTitle}</title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithTitleHavingWhitespace_ShouldReturnTrimmedTitle()
    {
        // Arrange
        const string expectedTitle = "Test Page Title";
        const string htmlContent = $"<html><head><title>   {expectedTitle}   </title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithTitleHavingAttributes_ShouldReturnTitle()
    {
        // Arrange
        const string expectedTitle = "Test Page Title";
        const string htmlContent = $"<html><head><title class=\"page-title\" id=\"main-title\">{expectedTitle}</title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithMultilineTitleContent_ShouldReturnTitle()
    {
        // Arrange
        const string expectedTitle = "Test Page Title";
        const string htmlContent = $@"<html><head><title>
            {expectedTitle}
        </title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithNoTitleTag_ShouldReturnProvidedName()
    {
        // Arrange
        const string htmlContent = "<html><head></head><body><h1>No Title Tag</h1></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(name));
    }

    [Test]
    public async Task GetBookmarkTitle_WithEmptyTitle_ShouldReturnProvidedName()
    {
        // Arrange
        const string htmlContent = "<html><head><title></title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(name));
    }

    [Test]
    public async Task GetBookmarkTitle_WithWhitespaceOnlyTitle_ShouldReturnProvidedName()
    {
        // Arrange
        const string htmlContent = "<html><head><title>   </title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(name));
    }

    [Test]
    public async Task GetBookmarkTitle_WithHttpRequestException_ShouldReturnFailureResult()
    {
        // Arrange
        const string url = "https://example.com";
        const string name = "Example";
        var httpRequestException = new HttpRequestException("Network error", null, HttpStatusCode.NotFound);

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .Throws(httpRequestException);

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("HttpRequest exception occured"));
        Assert.That(result.Message, Does.Contain("NotFound"));
        Assert.That(result.Exception, Is.EqualTo(httpRequestException));
    }

    [Test]
    public async Task GetBookmarkTitle_WithTimeoutException_ShouldReturnFailureResult()
    {
        // Arrange
        const string url = "https://example.com";
        const string name = "Example";
        var timeoutException = new TimeoutException("Request timeout");
        var taskCanceledException = new TaskCanceledException("Operation was cancelled", timeoutException);

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .Throws(taskCanceledException);

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Timeout occurred"));
        Assert.That(result.Exception, Is.EqualTo(taskCanceledException));
    }

    [Test]
    public async Task GetBookmarkTitle_WithTaskCanceledExceptionNotTimeout_ShouldReturnFailureResult()
    {
        // Arrange
        const string url = "https://example.com";
        const string name = "Example";
        var taskCanceledException = new TaskCanceledException("Operation was cancelled");

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .Throws(taskCanceledException);

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Operation was cancelled"));
        Assert.That(result.Exception, Is.EqualTo(taskCanceledException));
    }

    [Test]
    public async Task GetBookmarkTitle_WithGenericException_ShouldReturnFailureResult()
    {
        // Arrange
        const string url = "https://example.com";
        const string name = "Example";
        var genericException = new InvalidOperationException("Unexpected error");

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .Throws(genericException);

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Error occurred while attempting to lookup"));
        Assert.That(result.Exception, Is.EqualTo(genericException));
    }

    [Test]
    public async Task GetBookmarkTitle_WithCaseInsensitiveTitleTag_ShouldReturnTitle()
    {
        // Arrange
        const string expectedTitle = "Test Page Title";
        const string htmlContent = $"<html><head><TITLE>{expectedTitle}</TITLE></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithSpecialCharactersInTitle_ShouldReturnTitle()
    {
        // Arrange
        const string expectedTitle = "Test & Title with 'quotes' and \"double quotes\"";
        const string htmlContent = $"<html><head><title>{expectedTitle}</title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithUnicodeCharactersInTitle_ShouldReturnTitle()
    {
        // Arrange
        const string expectedTitle = "Test Title with Unicode: ñáéíóú 中文 🚀";
        const string htmlContent = $"<html><head><title>{expectedTitle}</title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_WithMultipleTitleTags_ShouldReturnFirstTitle()
    {
        // Arrange
        const string expectedTitle = "First Title";
        const string htmlContent = $"<html><head><title>{expectedTitle}</title><title>Second Title</title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        var result = await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetBookmarkTitle_VerifyHttpRequestMade()
    {
        // Arrange
        const string htmlContent = "<html><head><title>Test</title></head><body></body></html>";
        const string url = "https://example.com";
        const string name = "Example";

        _mockHandler.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, htmlContent, "text/html");

        // Act
        await _serviceAgent.GetBookmarkTitle(name, url);

        // Assert
        _mockHandler.VerifyRequest(HttpMethod.Get, url, Times.Once());
    }
}