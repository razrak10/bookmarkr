using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Spectre.Console;

namespace bookmarkr.Commands;

public class InteractiveCommandHandler : AsynchronousCommandLineAction
{
    public InteractiveCommandHandler()
    {

    }

    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        OnInteractiveCommand();
        return Task.FromResult(0);
    }

    private static void OnInteractiveCommand()
    {
        bool isRunning = true;
        while (isRunning)
        {
            AnsiConsole.Write(
                new FigletText("Bookmarkr")
                .Centered()
                .Color(Color.SteelBlue));

            string selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[blue] What do you want to do?[/]")
                .AddChoices([
                    "Export bookmarks to file",
                    "View Bookmarks",
                    "Exit Program"
                ])
            );

            switch (selectedOption)
            {
                case "Export bookmarks to file":
                    ExportBookmarks();
                    break;
                case "View Bookmarks":
                    ViewBookmarks();
                    break;
                default:
                    isRunning = false;
                    break;
            }
        }
    }
}
