using System;
using System.CommandLine;

namespace bookmarkr.Commands.Export;

public class ExportCommand : Command, ICommandAssigner
{
    private readonly ExportCommandHandler _handler;

    Option<FileInfo> outputFileOption = new Option<FileInfo>("file", ["--file", "-f"])
    {
        Required = true,
        Description = "The output file that will store the bookmarks",
    }.AcceptLegalFileNamesOnly();

    public ExportCommand(
        ExportCommandHandler handler,
        string name,
        string? description = null)
        : base(name, description)
    {
        _handler = handler;
    }

    public ExportCommand AddOptions()
    {
        this.Add(outputFileOption);

        return this;
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
