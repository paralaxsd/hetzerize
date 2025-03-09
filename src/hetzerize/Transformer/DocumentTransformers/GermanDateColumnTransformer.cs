using System.Globalization;
using Hetzerize.Csv.Models;
using Hetzerize.Extensions;

namespace Hetzerize.Transformer.DocumentTransformers;

sealed class GermanDateColumnTransformer(string sourceColName, string trgColName)
    : ColumnTransformer(sourceColName, trgColName)
{
    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    protected override void TransformContentsOf(CsvColumn column) => 
        column.Entries.Apply(e => e.Value = TransformToGermanDate(e.Value));

    static string TransformToGermanDate(string text)
    {
        var date = DateOnly.Parse(text, CultureInfo.InvariantCulture);
        return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
    }
}