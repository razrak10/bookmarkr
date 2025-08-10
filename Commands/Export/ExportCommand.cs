using System;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.Windows.Input;

namespace bookmarkr.Commands.Export;

public class ExportCommand : Command
{
    //TODO: Create options folder and classes
    Option<FileInfo> outputFileOption = new Option<FileInfo>("file", ["--file", "-f"])
    {
        Required = true,
        Description = "The output file that will store the bookmarks",
    }.AcceptLegalFileNamesOnly();

    public ExportCommand(string name, string? description = null) : base(name, description)
    {
    }

    public ExportCommand AddOptions()
    {
        this.Add(outputFileOption);

        return this;
    }

    public ExportCommand AssignCommandHandler()
    {
        this.UseCommandHandler<ExportCommandHandler>();

        return this;
    }
}
