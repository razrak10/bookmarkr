using System;
using System.CommandLine;

namespace bookmarkr.Commands;

public interface ICommandBuilder
{
    Command Build();
}
