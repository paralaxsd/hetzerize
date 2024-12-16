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
        var numColumns = csvHeader.NumEntries;

        if (csvLines.Any(line => line.NumEntries != numColumns))
        {
            throw new InvalidDataException("Invalid data: the given CSV data has varying entries per line.");
        }

        return new(csvHeader, csvLines[1..]);
    }

    CsvLine CreateCsvLineFrom(string line, int lineIdx)
    {
        var entries = line.Split(_delimiter);
        var csvEntries = entries.Select((val, idx) => new CsvEntry(idx, val));

        return new(lineIdx, csvEntries.ToArray());
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