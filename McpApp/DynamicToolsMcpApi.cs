using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

public sealed class ToolParameter
{
    public string Name { get; init; } = default!;
    public string Type { get; init; } = "string"; // string | int | bool | json
    public bool Required { get; init; }
}

public sealed class DynamicToolDefinition
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public IReadOnlyList<ToolParameter> Parameters { get; init; } = [];
    public string Script { get; init; } = default!;
}

public interface IDynamicToolRegistry
{
    DynamicToolDefinition? Get(string name);
    IReadOnlyCollection<DynamicToolDefinition> GetAll();
}

public sealed class InMemoryDynamicToolRegistry : IDynamicToolRegistry
{
    private readonly Dictionary<string, DynamicToolDefinition> _tools;

    public InMemoryDynamicToolRegistry()
    {
        _tools = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ReverseWords"] = new()
            {
                Name = "ReverseWords",
                Description = "Reverse order of words in text",
                Parameters =
                [
                    new ToolParameter { Name = "text", Required = true }
                ],
                Script = "ReverseWords"
            },

            ["Uppercase"] = new()
            {
                Name = "Uppercase",
                Description = "Convert text to upper case",
                Parameters =
                [
                    new ToolParameter { Name = "text", Required = true }
                ],
                Script = "Uppercase"
            }
        };
    }

    public DynamicToolDefinition? Get(string name)
        => _tools.TryGetValue(name, out var tool) ? tool : null;

    public IReadOnlyCollection<DynamicToolDefinition> GetAll()
        => _tools.Values;
}

public interface IDynamicToolExecutor
{
    string Execute(DynamicToolDefinition tool, IDictionary<string, object?> args);
}

public sealed class DynamicToolExecutor : IDynamicToolExecutor
{
    public string Execute(
        DynamicToolDefinition tool,
        IDictionary<string, object?> args)
    {
        return tool.Script switch
        {
            "ReverseWords" => ReverseWords(args),
            "Uppercase" => Uppercase(args),
            _ => throw new InvalidOperationException(
                $"Unknown script: {tool.Script}")
        };
    }

    private static string ReverseWords(IDictionary<string, object?> args)
    {
        var text = args["text"]?.ToString() ?? string.Empty;
        return string.Join(
            ' ',
            text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Reverse());
    }

    private static string Uppercase(IDictionary<string, object?> args)
    {
        return args["text"]?.ToString()?.ToUpperInvariant() ?? string.Empty;
    }
}

[McpServerToolType]
public static class DynamicToolsMcpApi
{
    private static readonly IDynamicToolRegistry _registry =
        new InMemoryDynamicToolRegistry();

    private static readonly IDynamicToolExecutor _executor =
        new DynamicToolExecutor();

    [McpServerTool]
    [DisplayName("ExecuteDynamicTool")]
    [Description("Executes a dynamically defined tool from registry")]
    public static string Execute(
        string toolName,
        string jsonArguments)
    {
        var tool = _registry.Get(toolName);
        if (tool is null)
            return $"Tool '{toolName}' not found";

        var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(
            jsonArguments,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];

        ValidateArguments(tool, args);

        return _executor.Execute(tool, args);
    }

    private static void ValidateArguments(
        DynamicToolDefinition tool,
        IDictionary<string, object?> args)
    {
        foreach (var param in tool.Parameters.Where(p => p.Required))
        {
            if (!args.ContainsKey(param.Name))
            {
                throw new ArgumentException(
                    $"Missing required parameter: {param.Name}");
            }
        }
    }
}