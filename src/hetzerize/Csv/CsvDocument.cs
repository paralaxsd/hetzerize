namespace Hetzerize.Csv;

sealed record CsvDocument(CsvLine Header, IReadOnlyList<CsvLine> Contents)
{
    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    public IEnumerable<CsvLine> AllLines => [Header, ..Contents];

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public IEnumerable<CsvEntry> GetEntriesInColumn(string columnName) =>
        Header.FindEntryWithValue(columnName) is { } colHeader ?
            GetEntriesInColumn(colHeader.ColumnIdx) : [];

    public IEnumerable<CsvEntry> GetEntriesInColumn(int columnIdx) =>
        Contents
            .Select(l => l[columnIdx])
            .Where(e => e != null)
            .Select(e => e!);
}

sealed record CsvLine(int LineIdx, IReadOnlyList<CsvEntry> Entries)
{
    public int NumEntries => Entries.Count;

    public CsvEntry? this[int index] =>
        Entries.ElementAtOrDefault(index);

    public CsvEntry? FindEntryWithValue(string value)
    {
        var quotedValue = $"\"{value}\"";
        return Entries.FirstOrDefault(e => 
            e.Value == value || e.Value == quotedValue);
    }
}

sealed class CsvEntry(int columnIdx, string value)
{
    public int ColumnIdx { get; } = columnIdx;
    public string Value { get; set; } = value;
}