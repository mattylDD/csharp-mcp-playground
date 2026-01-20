using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(DynamicToolsMcpApi).Assembly);

await builder.Build().RunAsync();

// --- 

// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using ModelContextProtocol.Server;

// var builder = WebApplication.CreateBuilder(args);

// builder.Logging.AddJsonConsole();
// builder.Logging.AddFilter("ModelContextProtocol", LogLevel.Trace);

// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//     });
// });

// builder.Services
//     .AddMcpServer()
//     .WithHttpTransport()
//     .WithToolsFromAssembly(typeof(TestTools).Assembly);

// Console.WriteLine("typeof(TestTools).Assembly.FullName");

// Console.WriteLine(typeof(TestTools).Assembly.FullName);
// Console.WriteLine("typeof(Program).Assembly.FullName");

// Console.WriteLine(typeof(Program).Assembly.FullName);

// builder.Logging.AddFilter("ModelContextProtocol.Server.ToolRegistry", LogLevel.Trace);

// var app = builder.Build();

// app.UseCors();
// app.MapMcp();

// app.Run("http://localhost:3333");
