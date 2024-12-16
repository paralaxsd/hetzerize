namespace Hetzerize.Transformer;

sealed record TransformerInputs(
    string CsvPath, 
    string SrcDelim, 
    string TrgDelim, 
    bool Force);