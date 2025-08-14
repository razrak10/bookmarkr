using bookmarkr.Commands.Category;
using System.CommandLine;

namespace bookmarkr.Commands.Sync;

public class CategoryCommand : Command, ICommandAssigner
{
    private readonly CategoryCommandHandler _categoryCommandHandler;

    public CategoryCommand(
        CategoryCommandHandler categoryCommandHandler,
        string name,
        string? description = null
    ) : base(name, description)
    {
        _categoryCommandHandler = categoryCommandHandler;
    }

    public Command AssignHandler(Action<ParseResult>? action = default)
    {
        if (action is not null)
        {
            this.SetAction(action);
        }
        else
        {
            this.SetAction(async (parseResult) =>
            {
                await _categoryCommandHandler.HandleAsync(parseResult);
            });
        }

        return this;
    }
}
