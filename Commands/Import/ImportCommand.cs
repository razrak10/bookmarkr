using System;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.Windows.Input;

namespace bookmarkr.Commands.Export;

public class ImportCommand : Command
{
    //TODO: Create options folder and classes
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

    public ImportCommand(string name, string? description = null) : base(name, description)
    {
    }

    public ImportCommand AddOptions(Option<FileInfo>? fileOption = null, Option<bool>? mergeOption = null)
    {
        this.Add(fileOption ?? inputFileOption);
        this.Add(mergeOption ?? mergeFileOption);

        return this;
    }

    public ImportCommand AssignCommandHandler()
    {
        this.UseCommandHandler<ImportCommandHandler>();

        return this;
    }
}
