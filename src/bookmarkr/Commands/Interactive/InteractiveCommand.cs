using System.CommandLine;

namespace bookmarkr.Commands.Interactive;

public class InteractiveCommand : Command, ICommandAssigner
{
    private readonly InteractiveCommandHandler _handler;

    public InteractiveCommand(
        InteractiveCommandHandler handler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _handler = handler;
    }

    public Command AssignHandler(Action<ParseResult>? action = default)
    {
        if (action is not null)
        {
            this.SetAction(action);
        }
        else
        {
            this.SetAction(async (parseResult) =>
            {
                await _handler.HandleAsync(parseResult);
            });
        }

        return this;
    }
}
