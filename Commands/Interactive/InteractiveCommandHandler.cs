using Spectre.Console;
using System.CommandLine;

namespace bookmarkr.Commands;

public class InteractiveCommandHandler
{
    private readonly BookMarkService _bookmarkService;

    public InteractiveCommandHandler(BookMarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        OnInteractiveCommand();
        return Task.FromResult(0);
    }

    private void OnInteractiveCommand()
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

    private void ExportBookmarks()
    {
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
        .Start(ctx =>
        {
            List<Bookmark> bookmarks = _bookmarkService.GetAll().ToList();

            ProgressTask task = ctx.AddTask("[yellow]exporting all bookmarks to file...[/]");

            task.MaxValue = bookmarks.Count;

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (Bookmark bookmark in bookmarks)
                {
                    _bookmarkService.ExportBookmark(bookmark, outputFilePath, writer);

                    task.Increment(1);

                    // Slow down process
                    Thread.Sleep(1000);
                }
            }
        });
        AnsiConsole.MarkupLine("[green]All bookmarks have been successfully exported[/]");
    }

    public void ViewBookmarks()
    {
        // Create tree
        Tree root = new Tree("Bookmarks");

        // Add some nodes
        TreeNode techBooksCategory = root.AddNode("[yellow]Tech Books[/]");
        TreeNode carsCategory = root.AddNode("[yellow]Cars[/]");
        TreeNode socialMediaCategory = root.AddNode("[yellow]Social Media[/]");
        TreeNode cookingCategory = root.AddNode("[yellow]Cooking[/]");

        // Add bookmarks for the Tech category
        var techBooks = _bookmarkService.GetBookmarksByCategory("Tech");
        foreach (Bookmark techBookmark in techBooks)
        {
            techBooksCategory.AddNode($"{techBookmark.Name} | {techBookmark.Url}");
        }

        var carsBooks = _bookmarkService.GetBookmarksByCategory("Cars");
        foreach (Bookmark carBook in carsBooks)
        {
            carsCategory.AddNode($"{carBook.Name} | {carBook.Url}");
        }

        var socialMediaBooks = _bookmarkService.GetBookmarksByCategory("SocialMedia");
        foreach (Bookmark socialMediaBook in socialMediaBooks)
        {
            socialMediaCategory.AddNode($"{socialMediaBook.Name} | {socialMediaBook.Url}");
        }

        var cookingBooks = _bookmarkService.GetBookmarksByCategory("Cooking");
        foreach (Bookmark cookingBook in cookingBooks)
        {
            cookingCategory.AddNode($"{cookingBook.Name} | {cookingBook.Url}");
        }

        // Render the tree
        AnsiConsole.Write(root);
    }
}
