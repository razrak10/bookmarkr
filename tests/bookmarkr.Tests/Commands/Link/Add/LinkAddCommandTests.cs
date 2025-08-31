using bookmarkr.Commands;
using bookmarkr.Commands.Link.Add;
using System.CommandLine;

namespace bookmarkr.Tests.Commands.Link.Add;

[TestFixture]
public class LinkAddCommandTests
{
    private LinkAddCommandHandler _handler;
    private LinkAddCommand _sut;
    private const string TestCommandName = "add";
    private const string TestCommandDescription = "Test link add command";

    [SetUp]
    public void SetUp()
    {
        _handler = new LinkAddCommandHandler(null!); // We'll need to mock IBookmarkService for full testing
        _sut = new LinkAddCommand(_handler, TestCommandName, TestCommandDescription);
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
        var handler = new LinkAddCommandHandler(null!);
        const string name = "test-add";
        const string description = "Test description";

        // Act
        var command = new LinkAddCommand(handler, name, description);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command, Is.InstanceOf<LinkAddCommand>());
        Assert.That(command, Is.InstanceOf<Command>());
        Assert.That(command, Is.InstanceOf<ICommandAssigner>());
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.EqualTo(description));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldCreateInstanceWithNullDescription()
    {
        // Arrange
        var handler = new LinkAddCommandHandler(null!);
        const string name = "test-add";

        // Act
        var command = new LinkAddCommand(handler, name, null);

        // Assert
        Assert.That(command, Is.Not.Null);
        Assert.That(command.Name, Is.EqualTo(name));
        Assert.That(command.Description, Is.Null);
    }

    [Test]
    public void Constructor_ShouldAddRequiredOptions()
    {
        // Assert
        Assert.That(_sut.Options.Count, Is.EqualTo(3));
        Assert.That(_sut.Options.Any(o => o.Name == "name"), Is.True);
        Assert.That(_sut.Options.Any(o => o.Name == "url"), Is.True);
        Assert.That(_sut.Options.Any(o => o.Name == "category"), Is.True);
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