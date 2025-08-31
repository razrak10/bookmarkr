using bookmarkr.Commands;
using bookmarkr.Commands.Interactive;
using System.CommandLine;

namespace bookmarkr.Tests.Commands.Interactive;

[TestFixture]
public class InteractiveCommandTests
{
    private InteractiveCommandHandler _handler;
    private InteractiveCommand _sut;
    private const string TestCommandName = "interactive";
    private const string TestCommandDescription = "Test interactive command";

    [SetUp]
    public void SetUp()
    {
        _handler = new InteractiveCommandHandler(null!); // We'll need to mock IBookmarkService for full testing
        _sut = new InteractiveCommand(_handler, TestCommandName, TestCommandDescription);
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
        var handler = new InteractiveCommandHandler(null!);
        const string name = "test-interactive";
        const string description = "Test description";

        // Act
        var command = new InteractiveCommand(handler, name, description);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command, Is.InstanceOf<InteractiveCommand>());
        Assert.That(command, Is.InstanceOf<Command>());
        Assert.That(command, Is.InstanceOf<ICommandAssigner>());
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.EqualTo(description));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldCreateInstanceWithNullDescription()
    {
        // Arrange
        var handler = new InteractiveCommandHandler(null!);
        const string name = "test-interactive";

        // Act
        var command = new InteractiveCommand(handler, name, null);

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
    public void AssignHandler_WithDefaultParameter_ShouldReturnSameInstance()
    {
        // Act
        var result = _sut.AssignHandler();

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
        var parseResult = rootCommand.Parse("interactive");
        await parseResult.InvokeAsync();

        // Assert
        Assert.That(customActionExecuted, Is.True);
    }

    [Test]
    public void ICommandAssigner_Interface_ShouldBeImplementedCorrectly()
    {
        // Act
        var commandAssigner = _sut as ICommandAssigner;

        // Assert
        Assert.That(commandAssigner, Is.Not.Null);
        Assert.That(commandAssigner.AssignHandler(), Is.SameAs(_sut));
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