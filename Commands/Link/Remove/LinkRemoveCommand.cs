using System.CommandLine;

namespace bookmarkr.Commands.Link.Remove;

public class LinkRemoveCommand : Command, ICommandAssigner
{
    private readonly LinkRemoveCommandHandler _handler;

    public LinkRemoveCommand(
        LinkRemoveCommandHandler handler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _handler = handler;

        // Add the name option
        var nameOption = new Option<string>("name", ["--name", "-n"]) { Required = true };
        this.Add(nameOption);
    }

    public Command AssignHandler(Action<ParseResult>? action = default)
    {
        if (action is not null)
        {
            this.SetAction(action);
        }
        else
        {
            this.SetAction((parseResult) =>
            {
                var task = _handler.HandleAsync(parseResult);
                return task.GetAwaiter().GetResult();
            });
        }

        return this;
    }
}
