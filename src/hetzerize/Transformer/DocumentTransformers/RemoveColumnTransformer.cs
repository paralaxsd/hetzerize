using Hetzerize.Csv.Models;

namespace Hetzerize.Transformer.DocumentTransformers;

sealed class RemoveColumnTransformer(string columnName) : ICsvDocumentTransformer
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly string _columnName = columnName;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void Transform(CsvDocument csvDoc) => csvDoc.RemoveColumnWith(_columnName);
}