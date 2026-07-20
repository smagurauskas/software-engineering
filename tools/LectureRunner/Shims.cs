// Replacements for the .NET Interactive display helpers used in the lecture
// notebooks (`.Display()`, `.DisplayTable()`). In the global namespace so
// lecture cells get them without a using directive, as in the notebooks.

using System.Collections;
using Microsoft.DotNet.Interactive.Formatting;

// Minimal stand-in for the deprecated .NET Interactive live-display API, used
// by the threading lecture's progress-bar helper.
namespace Microsoft.DotNet.Interactive.Formatting
{
    public class DisplayedValue
    {
        public void Update(object? value)
        {
            var text = value?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                Console.WriteLine(text);
            }
        }
    }
}

public static class NotebookShims
{
    // Notebook global function: display(value) shows a value and returns a
    // handle whose Update() replaces it in place. Here it prints instead.
    public static DisplayedValue display(object? value)
    {
        var displayed = new DisplayedValue();
        displayed.Update(value);
        return displayed;
    }

    public static T Display<T>(this T value)
    {
        if (value is string s)
        {
            Console.WriteLine(s);
        }
        else if (value is IEnumerable sequence)
        {
            foreach (var item in sequence)
            {
                Console.WriteLine(Render(item));
            }
        }
        else
        {
            Console.WriteLine(Render(value));
        }

        return value;
    }

    public static IEnumerable<T> DisplayTable<T>(this IEnumerable<T> rows)
    {
        foreach (var row in rows)
        {
            Console.WriteLine(Render(row));
        }

        return rows;
    }

    // Notebooks rendered property tables for plain objects; approximate that
    // with "Prop=Value" pairs when a type doesn't override ToString().
    private static string Render(object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        var type = value.GetType();
        var hasCustomToString =
            type.GetMethod("ToString", Type.EmptyTypes)?.DeclaringType != typeof(object);
        if (hasCustomToString || value is IEnumerable)
        {
            return value.ToString() ?? "<null>";
        }

        var properties = type.GetProperties()
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();
        if (properties.Count == 0)
        {
            return value.ToString() ?? "<null>";
        }

        var pairs = properties.Select(p => $"{p.Name}={p.GetValue(value)}");
        return string.Join(", ", pairs);
    }
}
