using Microsoft.VisualBasic;

namespace bookmarkr;

class Program
{
    static void Main(string[] args)
    {
        var rootCommand = new RootCommand()
        if (args == null && args.Length == 0)
        {
            Helper.ShowErrorMessage(["You have not passed any argument. Expected syntax is :", "bookmarkr <command-name> <parameters>"]);
            return;
        }

        var service = new BookMarkService();

        switch (args[0].ToLower())
        {
            case "link":
                ManageLinks(args, service);
                break;
            default:
                Helper.ShowErrorMessage(["Unkown Command"]);
                break;
        }
    }

    private static void ManageLinks(string[] args, BookMarkService service)
    {
        if (args.Length < 2)
        {
            Helper.ShowErrorMessage(["Unsufficient number of parameters. The expected syntax is:", "bookmarkr link <subcommand> <parameters>"]);
        }

        switch (args[1].ToLower())
        {
            case "add":
                service.AddLink(args[2], args[3]);
                break;
            default:
                Helper.ShowErrorMessage(["Unsufficient number of parameters. The expected syntax is:", "bookmarks link <subcommand> <parameter>"]);
                break;  
        }
    }
}
