using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public static class TestTools
{
    [McpServerTool, DisplayName("Echo"), Description("Echo message")]
    public static string Echo(string message) => $"Hello from C# MCP: {message}";

    [McpServerTool, DisplayName("Reverse"), Description("Reverse message")]
    public static string Reverse(string message)
    {
        if (message == null) return string.Empty;

        var chars = message.ToCharArray();

        Array.Reverse(chars);

        var reversed = new string(chars);

        return reversed;
    }

    [McpServerTool, DisplayName("Length"), Description("Length of message")]
    public static int Length(string message) => message?.Length ?? 0;
}