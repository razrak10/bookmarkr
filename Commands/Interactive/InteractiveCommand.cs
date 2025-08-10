using System;
using System.CommandLine;
using System.CommandLine.Hosting;

namespace bookmarkr.Commands.Interactive;

public class InteractiveCommand : Command
{
    public InteractiveCommand(string name, string? description = null) : base(name, description)
    {
    }

    public InteractiveCommand AddOptions()
    {
        return this;
    }

    public InteractiveCommand AssignCommandHandler()
    {
        this.UseCommandHandler<InteractiveCommandHandler>();

        return this;
    }
}
