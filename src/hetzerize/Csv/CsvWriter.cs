using Hetzerize.Csv.Models;
using Hetzerize.Extensions;

namespace Hetzerize.Csv;

sealed class CsvWriter(string delimiter)
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly string _delimiter = delimiter;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void WriteTo(StreamWriter writer, CsvDocument csvDoc) =>
        csvDoc.AllLines.Apply(line => WriteLine(writer, line));

    void WriteLine(StreamWriter writer, CsvLine line)
    {
        var entries = line.Entries;
        if (entries.Count == 0) { return; }

        if (entries.Count > 1)
        {
            entries.
                Take(entries.Count - 1)
                .Apply(e =>
                {
                    writer.Write(e.Value);
                    writer.Write(_delimiter);
                });
        }

        writer.WriteLine(entries[^1].Value);
    }
}