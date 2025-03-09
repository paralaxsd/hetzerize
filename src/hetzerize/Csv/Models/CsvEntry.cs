using System.Diagnostics;

namespace Hetzerize.Csv.Models;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
sealed class CsvEntry(string value)
{
    public string Value { get; set; } = value;
}