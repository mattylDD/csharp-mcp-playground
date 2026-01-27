using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public sealed class StringTools
{
    [McpServerTool, DisplayName("Reverse"), Description("Reverse message")]
    public static string Reverse(string message)
    {
        if (message == null) return string.Empty;

        Console.WriteLine($"\n\nMESSAGE: {message}\n\n");

        var chars = message.ToCharArray();

        Console.WriteLine($"\n\nchars: {chars}\n\n");

        Array.Reverse(chars);

        Console.WriteLine($"\n\nreversed chars: {chars}\n\n");

        var reversed = new string(chars);

        Console.WriteLine($"\n\nReversed MESSAGE: {reversed}\n\n");

        return reversed;
    }

    [McpServerTool, DisplayName("Length"), Description("Length of message")]
    public static int Length(string message) => message?.Length ?? 0;
}