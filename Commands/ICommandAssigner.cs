using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmarkr.Commands
{
    internal interface ICommandAssigner
    {
        Command AssignHandler(Action<ParseResult>? action = default);
    }
}
