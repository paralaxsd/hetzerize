using Hetzerize.Extensions;

namespace Hetzerize.Csv.Models;

sealed record CsvLine(IReadOnlyList<CsvEntry> Entries)
{
    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    public int NumEntries => Entries.Count;

    CsvEntry? this[int index] => Entries.ElementAtOrDefault(index);

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public CsvEntry GetElementAt(int index) => this[index].NotNull();
}