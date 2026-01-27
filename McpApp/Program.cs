using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Collections.Concurrent;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddJsonConsole();
builder.Logging.AddFilter("ModelContextProtocol", LogLevel.Trace);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Create and populate the tool dictionary at startup
var toolDictionary = new ConcurrentDictionary<string, McpServerTool[]>();
PopulateToolDictionary(toolDictionary);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Configure per-session options to filter tools based on route category
        options.ConfigureSessionOptions = async (httpContext, mcpOptions, cancellationToken) =>
        {
            // Determine tool category from route parameters
            var toolCategory = httpContext.Request.RouteValues["toolCategory"]?.ToString()?.ToLower() ?? "all";

            // Get pre-populated tools for the requested category
            if (toolDictionary.TryGetValue(toolCategory, out var tools))
            {
                mcpOptions.Capabilities = new();
                mcpOptions.Capabilities.Tools = new();
                var toolCollection = mcpOptions.ToolCollection = [];

                foreach (var tool in tools)
                {
                    toolCollection.Add(tool);
                }
            }
        };
    });

builder.Logging.AddFilter("ModelContextProtocol.Server.ToolRegistry", LogLevel.Trace);

var app = builder.Build();

app.UseCors();

// Map MCP with route parameter for tool category filtering
app.MapMcp("/{toolCategory?}");

app.Run("http://localhost:3333");

// Helper method to populate the tool dictionary at startup
static void PopulateToolDictionary(ConcurrentDictionary<string, McpServerTool[]> toolDictionary)
{
    // Get tools for each category
    var stringTools = GetToolsForType<StringTools>();
    var echoTools = GetToolsForType<EchoTools>();
    McpServerTool[] allTools = [.. stringTools,
                                .. echoTools];

    // Populate the dictionary with tools for each category
    toolDictionary.TryAdd("string", stringTools);
    toolDictionary.TryAdd("echo", echoTools);
    toolDictionary.TryAdd("all", allTools);
}

// Helper method to get tools for a specific type using reflection
static McpServerTool[] GetToolsForType<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
    System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicMethods)] T>()
{
    var tools = new List<McpServerTool>();
    var toolType = typeof(T);
    var methods = toolType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());

    foreach (var method in methods)
    {
        try
        {
            var tool = McpServerTool.Create(method, target: null, new McpServerToolCreateOptions());
            tools.Add(tool);
        }
        catch (Exception ex)
        {
            // Log error but continue with other tools
            Console.WriteLine($"Failed to add tool {toolType.Name}.{method.Name}: {ex.Message}");
        }
    }

    return [.. tools];
}