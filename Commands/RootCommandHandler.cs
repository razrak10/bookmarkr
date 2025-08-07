using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr.Commands;

public class RootCommandHandler : AsynchronousCommandLineAction
{
    public RootCommandHandler()
    {

    }
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        return 0;
    }
}
