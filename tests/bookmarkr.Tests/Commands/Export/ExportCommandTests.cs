using bookmarkr.Commands;
using bookmarkr.Commands.Export;
using System.CommandLine;

namespace bookmarkr.Tests.Commands.Export;

[TestFixture]
public class ExportCommandTests
{
    private ExportCommandHandler _handler;
    private ExportCommand _sut;
    private const string TestCommandName = "export";
    private const string TestCommandDescription = "Test export command";

    [SetUp]
    public void SetUp()
    {
        _handler = new ExportCommandHandler(null!); // We'll need to mock IBookmarkService for full testing
        _sut = new ExportCommand(_handler, TestCommandName, TestCommandDescription);
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
        var handler = new ExportCommandHandler(null!);
        const string name = "test-export";
        const string description = "Test description";

        // Act
        var command = new ExportCommand(handler, name, description);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command, Is.InstanceOf<ExportCommand>());
        Assert.That(command, Is.InstanceOf<Command>());
        Assert.That(command, Is.InstanceOf<ICommandAssigner>());
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.EqualTo(description));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldCreateInstanceWithNullDescription()
    {
        // Arrange
        var handler = new ExportCommandHandler(null!);
        const string name = "test-export";

        // Act
        var command = new ExportCommand(handler, name, null);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.Null);
    }

    [Test]
    public void AddOptions_ShouldReturnSameInstance()
    {
        // Act
        var result = _sut.AddOptions();

        // Assert
        Assert.That(result, Is.SameAs(_sut));
        Assert.That(result, Is.InstanceOf<ExportCommand>());
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
        Action<ParseResult> customAction = _ => { };

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
        var parseResult = rootCommand.Parse("export");
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