using System;
using System.CommandLine;

namespace bookmarkr.Options;

public class NameOption : Option<string[]>
{
    public NameOption(
        string name,
        string[] aliases,
        ArgumentArity arity,
        bool isRequired = true,
        bool allowMultipleArgumentsPerToken = true
        ) : base(name, aliases)
    {
        this.Required = isRequired;
        this.AllowMultipleArgumentsPerToken = allowMultipleArgumentsPerToken;
        this.Arity = arity;
    }
}
