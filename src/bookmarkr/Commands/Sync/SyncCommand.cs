using System.CommandLine;

namespace bookmarkr.Commands.Sync;

public class SyncCommand : Command
{
    private readonly SyncCommandHandler _syncCommandHandler;

    public SyncCommand(
        SyncCommandHandler syncCommandHandler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _syncCommandHandler = syncCommandHandler;
    }

    public SyncCommand AssignAction(Action<ParseResult>? action = default)
    {
        if (action is not null)
        {
            this.SetAction(action);
        }
        else
        {
            this.SetAction(async (parseResult) =>
            {
                await _syncCommandHandler.HandleAsync(parseResult);
            });
        }

        return this;
    }
}
