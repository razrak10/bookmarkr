using System.CommandLine;

namespace bookmarkr.Commands.Import;

public class ImportCommand : Command, ICommandAssigner
{
    private readonly ImportCommandHandler _handler;

    private Option<FileInfo> inputFileOption = new Option<FileInfo>("file", ["--file", "-f"])
    {
        Required = true,
        Description = "The input file that contains the bookmarks to be imported",
    }
    .AcceptLegalFileNamesOnly()
    .AcceptExistingOnly();

    private Option<bool> mergeFileOption = new Option<bool>("merge", ["--merge", "-m"])
    {
        Description = "Import file option to merge imported bookmarks with existing ones.",
        Arity = ArgumentArity.Zero
    };

    public ImportCommand(
        ImportCommandHandler handler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _handler = handler;
    }

    public ImportCommand AddOptions(Option<FileInfo>? fileOption = null, Option<bool>? mergeOption = null)
    {
        this.Add(fileOption ?? inputFileOption);
        this.Add(mergeOption ?? mergeFileOption);

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
            this.SetAction((parseResult) =>
            {
                return _handler.HandleAsync(parseResult);
            });
        }

        return this;
    }
}
