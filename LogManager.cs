using System;
using System.Text;
using Serilog;

namespace bookmarkr;

public static class LogManager
{
    public static void LogError(string message, Exception? ex = null)
    {
        Log.Information(ex, message);
    }
}
