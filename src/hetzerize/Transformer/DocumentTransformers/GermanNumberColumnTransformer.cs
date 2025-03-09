using Hetzerize.Csv.Models;
using Hetzerize.Extensions;

namespace Hetzerize.Transformer.DocumentTransformers;

sealed class GermanNumberColumnTransformer(string sourceColName, string trgColName)
    : ColumnTransformer(sourceColName, trgColName)
{
    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    protected override void TransformContentsOf(CsvColumn column) => 
        column.Entries.Apply(e => e.Value = TransformToGermanDecimal(e.Value));

    static string TransformToGermanDecimal(string text) => text.Replace(".", ",");
}