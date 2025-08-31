using System.CommandLine;

namespace bookmarkr.Commands
{
    public interface ICommandAssigner
    {
        Command AssignHandler(Action<ParseResult>? action = default);
    }
}
