using System;
using System.CommandLine;

namespace bookmarkr.Options;

public class CategoryOption : Option<string[]>
{
    public CategoryOption(
        string name,
        string[] aliases,
        ArgumentArity arity,
        string[] defaultValues,
        bool isRequired = false,
        bool allowMultipleArgumentsPerToken = true
        ) : base(name, aliases)
    {
        this.Required = isRequired;
        this.AllowMultipleArgumentsPerToken = allowMultipleArgumentsPerToken;
        this.Arity = arity;
        this.DefaultValueFactory = (_) => defaultValues;
    }

    public CategoryOption AddDefaultValidators(IEnumerable<string> categories)
    {
        this.Validators.Add(result =>
        {
            var categories = result.GetValueOrDefault<string[]>();

            foreach (string category in categories)
            {
                if (!string.IsNullOrEmpty(category) && !categories.Contains(category))
                {
                    result.AddError($"Category must be one of: {string.Join(", ", categories)}");
                }
            }
        });

        return this;
    }

    public CategoryOption AddDefaultCompletionSources(IEnumerable<string> categories)
    {
        return this.AddDefaultCompletionSources(categories);
    }

}
