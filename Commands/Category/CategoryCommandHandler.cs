using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr.Commands.Category
{

    public class CategoryCommandHandler : AsynchronousCommandLineAction
    {
        public CategoryCommandHandler()
        {

        }

        public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
