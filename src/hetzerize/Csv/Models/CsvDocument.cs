using Hetzerize.Extensions;

namespace Hetzerize.Csv.Models;

sealed class CsvDocument(IEnumerable<CsvColumn> columns)
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly List<CsvColumn> _columns = columns.ToList();

    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    public IEnumerable<CsvLine> AllLines => [Header, .. Contents];
    public int NumContentEntries => _columns.First().Entries.Length;

    CsvLine Header => GetLineAt(0);
    IEnumerable<CsvLine> Contents => 
        Enumerable.Range(1, NumContentEntries - 1)
        .Select(GetLineAt);


    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public CsvColumn GetColumnWith(string name) => FindColumnWith(name)
        .NotNull($"Column '{name}' not found.");

    public bool HasColumnWith(string name) => FindColumnWith(name) is { };

    public void InsertColumnAt(int idx, CsvColumn column) =>
        _columns.Insert(idx, column);

    public void RemoveColumnWith(string name) =>
        _columns.Remove(GetColumnWith(name));

    public void MoveColumnTo(string columnName, int targetIdx)
    {
        var col = GetColumnWith(columnName);
        _columns.Remove(col);
        _columns.Insert(targetIdx, col);
    }

    CsvLine GetLineAt(int lineIdx)
    {
        var entries = _columns
            .Select(col => col.NameAndEntries.ElementAt(lineIdx))
            .ToArray();
        return new(entries);
    }

    CsvColumn? FindColumnWith(string name)
    {
        var quotedName = $"\"{name}\"";
        return _columns.FirstOrDefault(c => 
            c.Name == name || c.Name == quotedName);
    }
}