namespace Hetzerize.Transformer.DocumentTransformers;

sealed class InsertConstantDataColumnTransformer
    (string columnName, int idx, string text) 
    : InsertColumnTransformer(columnName, idx, _ => text);
