using System.CommandLine;

namespace bookmarkr.Commands.Show;

public class ShowCommand : Command, ICommandAssigner
{
    private readonly ShowCommandHandler _handler;

    public ShowCommand(
        ShowCommandHandler handler,
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
                return _handler.HandleAsync(parseResult);
            });
        }

        return this;
    }
}
