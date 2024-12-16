namespace Hetzerize.Transformer;

class CsvTransformer(TransformerInputs inputs)
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly FileInfo _csvFile = new(inputs.CsvPath);
    readonly string _srcDelim = inputs.SrcDelim;
    readonly string _trgDelim = inputs.TrgDelim;
    readonly bool _force = inputs.Force;

    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    FileInfo OutputPath => CreateOutputPath();

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public int Execute()
    {
        if(!_csvFile.Exists)
        {
            throw new FileNotFoundException($"File '{_csvFile.FullName}' not found.");
        }
        if(OutputPath.Exists && !_force)
        {
            throw new ArgumentException($"The output file '{OutputPath.FullName}' already exists. Use --force to overwrite it.");
        }

        var csvDoc = ReadCsvDocument();

        return 0;
    }

    CsvDocument ReadCsvDocument()
    {
        var csvReader = new CsvReader()
        {
            Delimiter = _srcDelim
        };
        using var streamReader = _csvFile.OpenText();
        return csvReader.ReadFrom(streamReader);
    }

    FileInfo CreateOutputPath()
    {
        var now = DateTime.Now;
        var prefix = $"{now.Year % 1000}{now.Month:D2}{now.Day:D2}";
        var filename = $"{prefix}_statement.csv";

        var outputPath = Path.Join(_csvFile.Directory!.FullName, filename);
        return new(outputPath);
    }
}
