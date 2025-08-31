using bookmarkr.Commands;
using bookmarkr.Commands.Change;
using System.CommandLine;

namespace bookmarkr.Tests.Commands.Change;

[TestFixture]
public class ChangeCommandTests
{
    private ChangeCommandHandler _handler;
    private ChangeCommand _sut;
    private const string TestCommandName = "change";
    private const string TestCommandDescription = "Test change command";

    [SetUp]
    public void SetUp()
    {
        _handler = new ChangeCommandHandler(null!); // We'll need to mock IBookmarkService for full testing
        _sut = new ChangeCommand(_handler, TestCommandName, TestCommandDescription);
    }

    [TearDown]
    public void TearDown()
    {
        _handler = null!;
        _sut = null!;
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var handler = new ChangeCommandHandler(null!);
        const string name = "test-change";
        const string description = "Test description";

        // Act
        var command = new ChangeCommand(handler, name, description);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command, Is.InstanceOf<ChangeCommand>());
        Assert.That(command, Is.InstanceOf<Command>());
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.EqualTo(description));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldCreateInstanceWithNullDescription()
    {
        // Arrange
        var handler = new ChangeCommandHandler(null!);
        const string name = "test-change";

        // Act
        var command = new ChangeCommand(handler, name, null);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.Null);
    }

    [Test]
    public void AssignHandler_WithNullAction_ShouldReturnSameInstance()
    {
        // Arrange
        Action<ParseResult>? nullAction = null;

        // Act
        var result = _sut.AssignHandler(nullAction);

        // Assert
        Assert.That(result, Is.SameAs(_sut));
        Assert.That(result, Is.InstanceOf<Command>());
    }

    [Test]
    public void AssignHandler_WithCustomAction_ShouldReturnSameInstance()
    {
        // Arrange
        var customActionCalled = false;
        Action<ParseResult> customAction = _ => { customActionCalled = true; };

        // Act
        var result = _sut.AssignHandler(customAction);

        // Assert
        Assert.That(result, Is.SameAs(_sut));
        Assert.That(result, Is.InstanceOf<Command>());
    }

    [Test]
    public async Task AssignHandler_WithCustomAction_ShouldUseCustomActionAsync()
    {
        // Arrange
        var customActionExecuted = false;
        var customAction = new Action<ParseResult>(_ => { customActionExecuted = true; });

        var rootCommand = new RootCommand();
        rootCommand.Add(_sut);
        _sut.AssignHandler(customAction);

        // Act
        var parseResult = rootCommand.Parse("change");
        await parseResult.InvokeAsync();

        // Assert
        Assert.That(customActionExecuted, Is.True);
    }

    [Test]
    public void Command_Properties_ShouldBeSetCorrectly()
    {
        // Assert
        Assert.That(_sut.Name, Is.EqualTo(TestCommandName));
        Assert.That(_sut.Description, Is.EqualTo(TestCommandDescription));
        Assert.That(_sut.Parents, Is.Empty);
    }
}