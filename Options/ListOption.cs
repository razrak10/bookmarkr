using System;
using System.CommandLine;

namespace bookmarkr.Options;

public class ListOption : Option<bool>
{
    public ListOption(
        string name,
        string[] aliases,
        bool isRequired = false)
        : base(name, aliases)
    {
    }
}
