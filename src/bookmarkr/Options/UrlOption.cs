using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace bookmarkr.Options;

public class UrlOption : Option<string[]>
{
    public UrlOption(
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

    public UrlOption AddDefaultValidators()
    {
        this.Validators.Add(result =>
        {
            foreach (Token token in result.Tokens)
            {
                if (string.IsNullOrWhiteSpace(token.Value))
                {
                    result.AddError("URL cannot be empty");
                    break;
                }
                else if (!Uri.TryCreate(token.Value, UriKind.Absolute, out _))
                {
                    result.AddError($"Invalid URL: {token.Value}");
                    break;
                }
            }
        });

        return this;
    }
}
