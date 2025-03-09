using Hetzerize.Csv.Models;

namespace Hetzerize.Transformer.DocumentTransformers;

/// <param name="targetIdx">Target column index after the original column was removed</param>
sealed class MoveColumnTransformer(string columnName, int targetIdx) : ICsvDocumentTransformer
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly string _columnName = columnName;
    readonly int _targetIdx = targetIdx;

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void Transform(CsvDocument csvDoc) => csvDoc.MoveColumnTo(_columnName, _targetIdx);
}