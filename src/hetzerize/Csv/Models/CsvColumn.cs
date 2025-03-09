using System.Diagnostics;

namespace Hetzerize.Csv.Models;

[DebuggerDisplay($"{{{nameof(Name)},nq}} ({{{nameof(Entries)}.Length,nq}} entries)")]
sealed class CsvColumn(string name, IEnumerable<string> entries)
{
    public string Name { get; set; } = name;
    public CsvEntry[] Entries { get; } =
        entries.Select(e => new CsvEntry(e)).ToArray();
    /// <summary>
    /// Same as <see cref="Entries"/> but also returns the header entry.
    /// </summary>
    public CsvEntry[] NameAndEntries => [ new(Name), .. Entries ];
}