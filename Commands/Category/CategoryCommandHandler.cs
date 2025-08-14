using System.CommandLine;

namespace bookmarkr.Commands.Category
{

    public class CategoryCommandHandler
    {
        public CategoryCommandHandler()
        {
        }

        public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
