using Hetzerize.Csv.Models;

namespace Hetzerize.Csv;

sealed class CsvReader(string delimiter = ";")
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly string _delimiter = delimiter;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public CsvDocument ReadFrom(StreamReader reader)
    {
        var lines = ReadLinesFrom(reader);

        if (lines.Count == 0)
        {
            throw new ArgumentException("The given CSV file has no readable content.");
        }

        var csvLines = lines.Select(CreateCsvLineFrom).ToArray();
        var csvHeader = csvLines[0];
        ValidateIntegrityOf(csvLines, csvHeader);

        var columns = GetColumnsFrom(csvLines);
        return new(columns);
    }
    
    CsvLine CreateCsvLineFrom(string line, int lineIdx)
    {
        var entries = line.Split(_delimiter);
        var csvEntries = entries.Select(val => new CsvEntry(val));

        return new(csvEntries.ToArray());
    }
    
    static void ValidateIntegrityOf(CsvLine[] csvLines, CsvLine csvHeader)
    {
        var numColumns = csvHeader.NumEntries;

        if (csvLines.Any(line => line.NumEntries != numColumns))
        {
            throw new InvalidDataException(
                "Invalid data: the given CSV data has varying entries per line.");
        }
    }

    static IEnumerable<CsvColumn> GetColumnsFrom(CsvLine[] lines)
    {
        var header = lines[0];
        var contentLines = lines.Skip(1).ToArray();
        var numColumns = header.NumEntries;

        foreach (var colIdx in Enumerable.Range(0, numColumns))
        {
            var colName = header.GetElementAt(colIdx).Value;
            var entries = contentLines
                .Select(l => l.GetElementAt(colIdx).Value);

            yield return new(colName, entries);
        }
    }

    static List<string> ReadLinesFrom(StreamReader reader)
    {
        List<string> lines = [];

        while (true)
        {
            var curLine = reader.ReadLine();
            if (curLine is { })
            {
                lines.Add(curLine);
                continue;
            }
            break;
        }

        return lines;
    }
}