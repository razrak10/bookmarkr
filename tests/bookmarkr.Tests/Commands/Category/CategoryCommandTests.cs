using bookmarkr.Commands;
using bookmarkr.Commands.Category;
using bookmarkr.Commands.Sync;
using System.CommandLine;

namespace bookmarkr.Tests.Commands.Category;

[TestFixture]
public class CategoryCommandTests
{
    private CategoryCommandHandler _handler;
    private CategoryCommand _sut;
    private const string TestCommandName = "category";
    private const string TestCommandDescription = "Test category command";

    [SetUp]
    public void SetUp()
    {
        _handler = new CategoryCommandHandler();
        _sut = new CategoryCommand(_handler, TestCommandName, TestCommandDescription);
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
        var handler = new CategoryCommandHandler();
        const string name = "test-category";
        const string description = "Test description";

        // Act
        var command = new CategoryCommand(handler, name, description);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command, Is.InstanceOf<CategoryCommand>());
        Assert.That(command, Is.InstanceOf<Command>());
        Assert.That(command, Is.InstanceOf<ICommandAssigner>());
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.EqualTo(description));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldCreateInstanceWithNullDescription()
    {
        // Arrange
        var handler = new CategoryCommandHandler();
        const string name = "test-category";

        // Act
        var command = new CategoryCommand(handler, name, null);

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
        _sut.AssignHandler(customAction);

        // Act
        var parseResult = rootCommand.Parse("category");
        await parseResult.InvokeAsync();

        // Assert
        Assert.That(customActionExecuted, Is.True);
    }

    [Test]
    public void AssignHandler_CalledMultipleTimes_ShouldReturnSameInstanceEachTime()
    {
        // Act
        var result1 = _sut.AssignHandler();
        var result2 = _sut.AssignHandler();
        var result3 = _sut.AssignHandler();

        // Assert
        Assert.That(result1, Is.SameAs(_sut));
        Assert.That(result2, Is.SameAs(_sut));
        Assert.That(result3, Is.SameAs(_sut));
        Assert.That(result1, Is.SameAs(result2));
        Assert.That(result2, Is.SameAs(result3));
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

    [Test]
    public async Task AssignHandler_WithDefaultAction_ShouldInvokeSuccessfully()
    {
        // Arrange
        var rootCommand = new RootCommand();
        rootCommand.Add(_sut);
        _sut.AssignHandler();

        // Act
        var parseResult = rootCommand.Parse("category");
        var exitCode = await parseResult.InvokeAsync();

        // Assert
        Assert.That(exitCode, Is.EqualTo(0));
    }
}