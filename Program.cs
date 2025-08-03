using System.CommandLine;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace bookmarkr;

class Program
{
    private BookMarkService _service = new BookMarkService();

    static int Main(string[] args)
    {


        RootCommand rootCommand = new RootCommand("Bookmarkr is a bookmark manager provided as a CLI application");

        rootCommand.SetAction(parseResult =>
        {
            OnHandleRootCommand();
            return 0;
        });

        var linkCommand = new Command("link", "Manage bookmarks links");

        rootCommand.Add(linkCommand);

        var nameOption = new Option<string>("name", ["--name", "-n"]);
        var urlOption = new Option<string>("url", ["--url", "-u"]);

        var addLinkCommand = new Command("add", "Add a new bookmark link");
        addLinkCommand.Add(nameOption);
        addLinkCommand.Add(urlOption);

        linkCommand.Add(addLinkCommand);

        addLinkCommand.SetAction(parseResult =>
        {
            var name = parseResult.GetValue<string>("name");
            var url = parseResult.GetValue<string>("url");

            OnHandleAddLinkCommand(name, url);
            return 0;
        });

        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    private static void OnHandleRootCommand()
    {
        Console.WriteLine("Hello from the root command!");
    }

    private static void OnHandleAddLinkCommand(string name, string url)
    {
        Console.WriteLine("hello again");
        var service = new BookMarkService();
        service.AddLink(name, url);
    }
}
