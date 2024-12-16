using ConsoleAppFramework;
using Hetzerize.ArgumentHandling;
using Spectre.Console;

namespace Hetzerize;

static class Program
{
    static void Main(string[] args)
    {
        ConsoleApp.LogError = LogErrorAsMarkup;

        var app = ConsoleApp.Create();
        app.Add<Commands>();
        app.Run(args);
    }

    static void LogErrorAsMarkup(string text) => AnsiConsole.MarkupLine($"[red]{text}[/]");
}