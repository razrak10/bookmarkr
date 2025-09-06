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

        if (!techResult.IsSuccess)
        {
            string message = $"Error occured while retrieving categoy 'Tech'. Error: {techResult.Message}";
            LogManager.LogError(message, techResult.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }
        IEnumerable<Bookmark>? techBooks = techResult.Value;
        foreach (Bookmark techBookmark in techBooks!)
        {
            techBooksCategory.AddNode($"{techBookmark.Name} | {techBookmark.Url}");
        }

        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> carsResult = await _bookmarkService.GetBookmarksByCategory("Cars");
        if (!carsResult.IsSuccess)
        {
            string message = $"Error occured while retrieving categoy 'Cars'. Error: {carsResult.Message}";
            LogManager.LogError(message, carsResult.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }
        IEnumerable<Bookmark>? carsBooks = carsResult.Value;
        foreach (Bookmark carBook in carsBooks!)
        {
            carsCategory.AddNode($"{carBook.Name} | {carBook.Url}");
        }

        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> socialResult = await _bookmarkService.GetBookmarksByCategory("SocialMedia");
        if (!socialResult.IsSuccess)
        {
            string message = $"Error occured while retrieving categoy 'SocialMedia'. Error: {socialResult.Message}";
            LogManager.LogError(message, socialResult.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }
        IEnumerable<Bookmark>? socialBooks = socialResult.Value;
        foreach (Bookmark socialMediaBook in socialBooks!)
        {
            socialMediaCategory.AddNode($"{socialMediaBook.Name} | {socialMediaBook.Url}");
        }

        ExecutionResult.ExecutionResult<IEnumerable<Bookmark>> cookResult = await _bookmarkService.GetBookmarksByCategory("Cooking");
        if (!cookResult.IsSuccess)
        {
            string message = $"Error occured while retrieving categoy 'Cooking'. Error: {cookResult.Message}";
            LogManager.LogError(message, cookResult.Exception);
            MessageHelper.ShowErrorMessage([message]);
            return;
        }
        IEnumerable<Bookmark>? cookBooks = cookResult.Value;
        foreach (Bookmark cookingBook in cookBooks!)
        {
            cookingCategory.AddNode($"{cookingBook.Name} | {cookingBook.Url}");
        }

        // Render the tree
        AnsiConsole.Write(root);
    }
}
