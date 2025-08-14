using System.CommandLine;
using bookmarkr.Options;

namespace bookmarkr.Commands.Link;

public class LinkCommand : Command, ICommandAssigner
{
    private readonly LinkCommandHandler _handler;

    public LinkCommand(
        LinkCommandHandler handler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _handler = handler;

        // Add the list option
        var listOption = new ListOption("list", ["--list", "-l"]);
        this.Add(listOption);
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
                return _handler.HandleAsync(parseResult);
            });
        }

        return this;
    }
}
