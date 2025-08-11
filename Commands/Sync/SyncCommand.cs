using System.CommandLine;
using System.CommandLine.Hosting;

namespace bookmarkr.Commands.Sync;

public class SyncCommand : Command
{
    public SyncCommand(string name, string? description = null) : base(name, description)
    {
    }

    public SyncCommand AssignCommandHandler()
    {
        this.UseCommandHandler<SyncCommandHandler>();

        return this;
    }
}
