using System.CommandLine;
using bookmarkr.Options;

namespace bookmarkr.Commands.Link.Add;

public class LinkAddCommand : Command, ICommandAssigner
{
    private readonly LinkAddCommandHandler _handler;

    public LinkAddCommand(
        LinkAddCommandHandler handler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _handler = handler;

        // Add the required options
        var nameOption = new NameOption("name", ["--name", "-n"], arity: ArgumentArity.OneOrMore);
        var urlOption = new UrlOption("url", ["--url", "-u"], arity: ArgumentArity.OneOrMore).AddDefaultValidators();
        var categoryOption = new CategoryOption("category", ["--category", "-c"], arity: ArgumentArity.OneOrMore, defaultValues: ["Read later"])
            .AddDefaultValidators(["Read later", "Tech books", "Cooking", "Social media"])
            .AddDefaultCompletionSources(["Read later", "Tech books", "Cooking", "Social media"]);

        this.Add(nameOption);
        this.Add(urlOption);
        this.Add(categoryOption);
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
