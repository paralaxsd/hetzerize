using Hetzerize.Csv.Models;

namespace Hetzerize.Transformer.DocumentTransformers;

sealed class RenameColumnTransformer(string sourceColName, string trgColName) 
    : ColumnTransformer(sourceColName, trgColName)
{
    protected override void TransformContentsOf(CsvColumn column) { }
}