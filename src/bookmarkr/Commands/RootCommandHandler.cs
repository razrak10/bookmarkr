using System;
using System.CommandLine;

namespace bookmarkr.Commands;

public class RootCommandHandler
{
    public RootCommandHandler()
    {

    }

    public Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }
}
