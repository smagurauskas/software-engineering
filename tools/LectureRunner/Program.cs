// Executes the C# code cells of a converted lecture (.qmd) the same way the
// original .NET Interactive notebooks did: each ```{.csharp} block is one
// Roslyn scripting submission, chained so state carries across cells, later
// cells may shadow earlier variables, and a throwing cell prints the error
// and execution continues with the next cell.
//
// Usage: dotnet run --project tools/LectureRunner -- <lecture.qmd>
//
// Exit codes: 0 = all cells executed (runtime exceptions are notebook-normal
// and printed as output), 1 = a cell failed to COMPILE (code rot), 2 = usage.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: LectureRunner <lecture.qmd>");
    return 2;
}

// Deterministic output regardless of host machine settings.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
Console.SetIn(TextReader.Null);
Console.OutputEncoding = Encoding.UTF8;

var source = File.ReadAllText(args[0]);
var cells = Regex.Matches(source, "^```\\{\\.csharp\\}\r?\n(.*?)^```\\s*$",
        RegexOptions.Singleline | RegexOptions.Multiline)
    .Select(m => m.Groups[1].Value)
    .ToList();

if (cells.Count == 0)
{
    Console.Error.WriteLine($"No executable cells found in {args[0]}");
    return 0;
}

var options = ScriptOptions.Default
    .WithReferences(
        typeof(object).Assembly,
        typeof(Console).Assembly,
        typeof(Enumerable).Assembly,
        typeof(ParallelEnumerable).Assembly,
        typeof(HttpClient).Assembly,
        typeof(Regex).Assembly,
        typeof(Stopwatch).Assembly,
        typeof(ConcurrentBag<>).Assembly,
        typeof(System.Text.Json.JsonSerializer).Assembly,
        typeof(System.ComponentModel.Component).Assembly,
        typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly,
        typeof(NotebookShims).Assembly)
    .WithImports(
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Collections.Concurrent",
        "System.Diagnostics",
        "System.Globalization",
        "System.IO",
        "System.Linq",
        "System.Net.Http",
        "System.Text",
        "System.Threading",
        "System.Threading.Tasks",
        // using-static of the shim class: notebook globals like display()
        "NotebookShims");

ScriptState<object>? state = null;
var compileFailures = 0;

for (var i = 0; i < cells.Count; i++)
{
    Console.WriteLine($"── cell {i + 1} ──");
    try
    {
        state = state is null
            ? await CSharpScript.RunAsync(cells[i], options)
            : await state.ContinueWithAsync(cells[i]);

        // Mirror notebook behaviour of displaying a trailing expression value.
        if (state.ReturnValue is not null)
        {
            state.ReturnValue.Display();
        }
    }
    catch (CompilationErrorException e)
    {
        // Notebooks show the compile error and keep going; so do we, but a
        // compile error usually means the material has rotted, so fail the run.
        Console.WriteLine($"[compile error] {e.Diagnostics.FirstOrDefault()}");
        compileFailures++;
    }
    catch (Exception e)
    {
        // Runtime exceptions are part of the lecture material (deliberately
        // thrown examples); print them the way a notebook would and continue.
        Console.WriteLine($"[exception] {e.GetType().Name}: {e.Message}");
    }
}

return compileFailures > 0 ? 1 : 0;
