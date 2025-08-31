using System.CommandLine;
using bookmarkr.Options;

namespace bookmarkr.Commands.Link.Update;

public class LinkUpdateCommand : Command, ICommandAssigner
{
    private readonly LinkUpdateCommandHandler _handler;

    public LinkUpdateCommand(
        LinkUpdateCommandHandler handler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _handler = handler;

        // Add the required options
        var nameOption = new NameOption("name", ["--name", "-n"], arity: ArgumentArity.OneOrMore);
        var urlOption = new UrlOption("url", ["--url", "-u"], arity: ArgumentArity.OneOrMore).AddDefaultValidators();

        this.Add(nameOption);
        this.Add(urlOption);
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
