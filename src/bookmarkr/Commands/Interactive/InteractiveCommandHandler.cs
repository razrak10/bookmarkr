using bookmarkr.Helpers;
using bookmarkr.Logger;
using bookmarkr.Service;
using Spectre.Console;
using System.CommandLine;
using System.Text.Json;

namespace bookmarkr.Commands;

public class InteractiveCommandHandler
{
    private readonly IBookmarkService _bookmarkService;

    public InteractiveCommandHandler(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    public async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        await OnInteractiveCommand();
        return 0;
    }

    private async Task OnInteractiveCommand()
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
                    await ViewBookmarks();
                    break;
                default:
                    isRunning = false;
                    break;
            }
        }
    }

    private void ExportBookmarks()
    {
        bool isSuccess = true;

        string outputFilePath = AnsiConsole.Prompt(
            new TextPrompt<string>("Please provide the output file name(default: 'bookmarks.json')")
            .DefaultValue("bookmarks2.json")
        );

        AnsiConsole.Progress()
        .AutoRefresh(true)
        .AutoClear(false)       // Avoids removing task list when completed
        .HideCompleted(false)   // Avoids hiding tasks as they are
        .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn()
            ])
        .Start(async ctx =>
        {
            ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> result = await _bookmarkService.GetBookmarksAsync(false);

            if (!result.IsSuccess)
            {
                string message = $"Error occured while exporting bookmarks. Error: {result.Message}";
                LogManager.LogError(message, result.Exception);
                MessageHelper.ShowErrorMessage([message]);
                isSuccess = false;
                return;
            }

            IEnumerable<Bookmark>? bookmarks = result.Value!;
            if (!bookmarks.Any())
            {
                string message = "No bookmarks currently present.";
                LogManager.LogInformation(message);
                MessageHelper.ShowWarningMessage([message]);
                isSuccess = false;
                return;
            }

            ProgressTask task = ctx.AddTask("[yellow]exporting all bookmarks to file...[/]");

            task.MaxValue = bookmarks!.Count();

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (Bookmark bookmark in bookmarks)
                {
                    writer.WriteLine(JsonSerializer.Serialize(bookmark));

                    task.Increment(1);

                    // Slow down process
                    Thread.Sleep(50);
                }
            }
        });

        if (isSuccess)
        {
            MessageHelper.ShowSuccessMessage(["All bookmarks have been successfully exported"]);
        }
    }

    public async Task ViewBookmarks()
    {
        // Create tree
        Tree root = new Tree("Bookmarks");

        // Add some nodes
        TreeNode techBooksCategory = root.AddNode("[yellow]Tech Books[/]");
        TreeNode carsCategory = root.AddNode("[yellow]Cars[/]");
        TreeNode socialMediaCategory = root.AddNode("[yellow]Social Media[/]");
        TreeNode cookingCategory = root.AddNode("[yellow]Cooking[/]");

        // Add bookmarks for the Tech category
        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> techResult = await _bookmarkService.GetBookmarksByCategory("Tech");

        if (techResult.IsSuccess && techResult.Value != null)
        {
            foreach (Bookmark techBookmark in techResult.Value)
            {
                techBooksCategory.AddNode($"{techBookmark.Name} | {techBookmark.Url}");
            }
        }
        else
        {
            techBooksCategory.AddNode("[grey](no bookmarks)[/]");
        }

        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> carsResult = await _bookmarkService.GetBookmarksByCategory("Cars");
        if (carsResult.IsSuccess && carsResult.Value != null)
        {
            foreach (Bookmark carBook in carsResult.Value)
            {
                carsCategory.AddNode($"{carBook.Name} | {carBook.Url}");
            }
        }
        else
        {
            carsCategory.AddNode("[grey](no bookmarks)[/]");
        }

        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> socialResult = await _bookmarkService.GetBookmarksByCategory("SocialMedia");
        if (socialResult.IsSuccess && socialResult.Value != null)
        {
            foreach (Bookmark socialMediaBook in socialResult.Value)
            {
                socialMediaCategory.AddNode($"{socialMediaBook.Name} | {socialMediaBook.Url}");
            }
        }
        else
        {
            socialMediaCategory.AddNode("[grey](no bookmarks)[/]");
        }

        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> cookResult = await _bookmarkService.GetBookmarksByCategory("Cooking");
        if (cookResult.IsSuccess && cookResult.Value != null)
        {
            foreach (Bookmark cookingBook in cookResult.Value)
            {
                cookingCategory.AddNode($"{cookingBook.Name} | {cookingBook.Url}");
            }
        }
        else
        {
            cookingCategory.AddNode("[grey](no bookmarks)[/]");
        }

        // Render the tree
        AnsiConsole.Write(root);
    }
}
