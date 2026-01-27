using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public sealed class EchoTools
{
    [McpServerTool, DisplayName("Echo"), Description("Echo message")]
    public static string Echo(string message) => $"Hello from C# MCP: {message}";

    [McpServerTool, DisplayName("CurrentTime"), Description("Show current time")]
    public static string CurrentTime() => $"Datetime now: {DateTime.Now}";
}