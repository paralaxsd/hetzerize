using Hetzerize.Csv.Models;

namespace Hetzerize.Transformer.DocumentTransformers;

class InsertColumnTransformer(string columnName, int insIdx, Func<int, string> createElementAt) 
    : ICsvDocumentTransformer
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly string _columnName = columnName;
    readonly int _insIdx = insIdx;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    Func<int, string> CreateElementAt { get; } = createElementAt;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void Transform(CsvDocument csvDoc)
    {
        if(csvDoc.HasColumnWith(_columnName))
        {
            throw new InvalidOperationException($"Column with name '{_columnName}' already exists.");
        }

        var numEntries = csvDoc.NumContentEntries;
        var entries = Enumerable.Range(0, numEntries)
            .Select(CreateElementAt)
            .ToArray();
        var column = new CsvColumn(_columnName, entries);
        csvDoc.InsertColumnAt(_insIdx, column);
    }
}