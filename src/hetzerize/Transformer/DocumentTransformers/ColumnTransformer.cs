using Hetzerize.Csv.Models;

namespace Hetzerize.Transformer.DocumentTransformers;

abstract class ColumnTransformer(string sourceColName, string trgColName) 
    : ICsvDocumentTransformer
{
    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    string SourceColumnName { get; } = sourceColName;
    string TargetColumnName { get; } = trgColName;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void Transform(CsvDocument csvDoc)
    {
        var column = csvDoc.GetColumnWith(SourceColumnName);
        column.Name = TargetColumnName;
        TransformContentsOf(column);
    }

    protected abstract void TransformContentsOf(CsvColumn column);
}