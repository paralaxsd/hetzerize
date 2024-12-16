namespace Hetzerize.Transformer;

sealed record CsvDocument(CsvLine Header, IReadOnlyList<CsvLine> Contents)
{
}

sealed record CsvLine(int LineIdx, IReadOnlyList<CsvEntry> Entries)
{
    public int NumEntries => Entries.Count;
}
readonly record struct CsvEntry(int ColumnIdx, string Value);