using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace bookmarkr.Commands;

public class RootCommandHandler : AsynchronousCommandLineAction
{
    public RootCommandHandler()
    {

    }
    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}
