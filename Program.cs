using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace bookmarkr;

class Program
{
    static void Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand("Bookmarkr is a bookmark manager provided as a CLI application");

        rootCommand.Set;

        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .Build();
    }

    private static void OnHandleRootCommand()
    {
        Console.WriteLine("Hello from the root command!");
    }
}
