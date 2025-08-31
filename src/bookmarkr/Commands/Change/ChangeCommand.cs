using System.CommandLine;

namespace bookmarkr.Commands.Change
{
    public class ChangeCommand : Command
    {
        private readonly ChangeCommandHandler _handler;

        public ChangeCommand(
            ChangeCommandHandler handler,
            string name, 
            string? description = null
        ) : base(name, description)
        {
            _handler = handler;
        }

        public Command AssignHandler(Action<ParseResult>? action)
        {
            if (action is not null)
            {
                this.SetAction(action);
            }
            else
            {
                this.SetAction(async (parseResult) =>
                {
                    await _handler.HandleAsync(parseResult);
                });
            }

            return this;
        }
    }
}
