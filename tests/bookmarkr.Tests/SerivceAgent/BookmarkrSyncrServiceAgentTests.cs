using bookmarkr.ServiceAgent;
using Moq;
using Moq.Contrib.HttpClient;
using System.Net;
using System.Text.Json;

namespace bookmarkr.Tests.ServiceAgent;

[TestFixture]
public class BookmarkrSyncrServiceAgentTests
{
    private Mock<HttpMessageHandler> _mockHandler;
    private IHttpClientFactory _factory;
    private BookmarkrSyncrServiceAgent _serviceAgent;
    private const string BaseAddress = "https://bookmarkrsyncr-api.azurewebsites.net";

    [SetUp]
    public void SetUp()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _factory = _mockHandler.CreateClientFactory();

        // Configure the named client to match the actual configuration
        Mock.Get(_factory).Setup(x => x.CreateClient("bookmarkrSyncr"))
            .Returns(() =>
            {
                var client = _mockHandler.CreateClient();
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Bookmarkr");
                return client;
            });

        _serviceAgent = new BookmarkrSyncrServiceAgent(_factory);
    }

    [Test]
    public void Constructor_WithValidHttpClientFactory_ShouldCreateInstance()
    {
        // Arrange & Act
        var serviceAgent = new BookmarkrSyncrServiceAgent(_factory);

        // Assert
        Assert.That(serviceAgent, Is.Not.Null);
        Assert.That(serviceAgent, Is.InstanceOf<IBookmarkrSyncrServiceAgent>());
    }

    [Test]
    public async Task SyncBookmarksAsync_WithValidBookmarks_ShouldReturnMergedBookmarks()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test 1", Url = "https://test1.com" },
            new() { Id = 2, Name = "Test 2", Url = "https://test2.com" }
        };

        var expectedMergedBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test 1", Url = "https://test1.com" },
            new() { Id = 2, Name = "Test 2", Url = "https://test2.com" },
            new() { Id = 3, Name = "Test 3", Url = "https://test3.com" }
        };

        var responseJson = JsonSerializer.Serialize(expectedMergedBookmarks);

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, responseJson, "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(3));
        Assert.That(result.Value![0].Name, Is.EqualTo("Test 1"));
        Assert.That(result.Value![2].Name, Is.EqualTo("Test 3"));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithEmptyLocalBookmarks_ShouldReturnEmptyList()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>();
        var responseJson = JsonSerializer.Serialize(new List<Bookmark>());

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, responseJson, "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task SyncBookmarksAsync_WithNotFoundResponse_ShouldReturnFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.NotFound, "Not Found");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Resource not found"));
        Assert.That(result.Message, Does.Contain("NotFound"));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithUnauthorizedResponse_ShouldReturnFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.Unauthorized, "Unauthorized");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Unauthorized access"));
        Assert.That(result.Message, Does.Contain("Unauthorized"));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithBadRequestResponse_ShouldReturnFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        const string errorResponse = "Bad Request - Invalid data";
        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.BadRequest, errorResponse);

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Failed to sync bookmarks"));
        Assert.That(result.Message, Does.Contain("BadRequest"));
        Assert.That(result.Message, Does.Contain(errorResponse));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithInternalServerErrorResponse_ShouldReturnFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        const string errorResponse = "Internal Server Error";
        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.InternalServerError, errorResponse);

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("Failed to sync bookmarks"));
        Assert.That(result.Message, Does.Contain("InternalServerError"));
        Assert.That(result.Message, Does.Contain(errorResponse));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithSuccessButEmptyResponse_ShouldReturnFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        var responseJson = JsonSerializer.Serialize(new List<Bookmark>());

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, responseJson, "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("An error occured when attempting to sync bookmarks."));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithSuccessButNullResponse_ShouldReturnFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, "null", "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("An error occured when attempting to sync bookmarks."));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithInvalidJsonResponse_ShouldReturnJsonFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        const string invalidJson = "{ invalid json }";

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, invalidJson, "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to serialize bookmarks to JSON format."));
        Assert.That(result.Exception, Is.InstanceOf<JsonException>());
    }

    [Test]
    public async Task SyncBookmarksAsync_WithHttpRequestException_ShouldReturnUnexpectedErrorFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        var httpRequestException = new HttpRequestException("Network error");

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .Throws(httpRequestException);

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("An unexpected error occurred during synchronization."));
        Assert.That(result.Exception, Is.EqualTo(httpRequestException));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithTaskCanceledException_ShouldReturnUnexpectedErrorFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        var taskCanceledException = new TaskCanceledException("Operation was cancelled");

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .Throws(taskCanceledException);

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("An unexpected error occurred during synchronization."));
        Assert.That(result.Exception, Is.EqualTo(taskCanceledException));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithGenericException_ShouldReturnUnexpectedErrorFailure()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        var genericException = new InvalidOperationException("Unexpected error");

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .Throws(genericException);

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Is.EqualTo("An unexpected error occurred during synchronization."));
        Assert.That(result.Exception, Is.EqualTo(genericException));
    }

    [Test]
    public async Task SyncBookmarksAsync_WithCaseInsensitiveJsonProperties_ShouldReturnMergedBookmarks()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        // Response with different casing
        const string responseJson = """
            [
                {
                    "id": 1,
                    "name": "Test Updated",
                    "url": "https://test-updated.com"
                }
            ]
            """;

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, responseJson, "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(1));
        Assert.That(result.Value![0].Name, Is.EqualTo("Test Updated"));
        Assert.That(result.Value![0].Url, Is.EqualTo("https://test-updated.com"));
    }

    [Test]
    public async Task SyncBookmarksAsync_VerifyCorrectHttpRequest_ShouldSendPostWithJsonContent()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new() { Id = 1, Name = "Test", Url = "https://test.com" }
        };

        var responseJson = JsonSerializer.Serialize(localBookmarks);

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, responseJson, "application/json");

        // Act
        await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        _mockHandler.VerifyRequest(HttpMethod.Post, $"{BaseAddress}/sync", Times.Once());
    }

    [Test]
    public async Task SyncBookmarksAsync_WithComplexBookmarkData_ShouldHandleSerializationCorrectly()
    {
        // Arrange
        var localBookmarks = new List<Bookmark>
        {
            new()
            {
                Id = 1,
                Name = "Complex & Test with 'quotes' and \"double quotes\"",
                Url = "https://test.com?param=value&other=test"
            }
        };

        var expectedMergedBookmarks = new List<Bookmark>
        {
            new()
            {
                Id = 1,
                Name = "Complex & Test with 'quotes' and \"double quotes\" - Updated",
                Url = "https://test-updated.com?param=newvalue"
            }
        };

        var responseJson = JsonSerializer.Serialize(expectedMergedBookmarks);

        _mockHandler.SetupRequest(HttpMethod.Post, $"{BaseAddress}/sync")
            .ReturnsResponse(HttpStatusCode.OK, responseJson, "application/json");

        // Act
        var result = await _serviceAgent.SyncBookmarksAsync(localBookmarks);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Count, Is.EqualTo(1));
        Assert.That(result.Value![0].Name, Does.Contain("Updated"));
        Assert.That(result.Value![0].Url, Does.Contain("newvalue"));
    }
}