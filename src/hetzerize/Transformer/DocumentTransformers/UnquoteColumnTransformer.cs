using Hetzerize.Csv.Models;
using Hetzerize.Extensions;

namespace Hetzerize.Transformer.DocumentTransformers;

sealed class UnquoteColumnTransformer(string columnName) : ICsvDocumentTransformer
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly string _columnName = columnName;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void Transform(CsvDocument csvDoc)
    {
        var column = csvDoc.GetColumnWith(_columnName);
        column.Entries.Apply(Unquote);
    }

    static void Unquote(CsvEntry entry) => entry.Value = entry.Value.Trim('"');
}