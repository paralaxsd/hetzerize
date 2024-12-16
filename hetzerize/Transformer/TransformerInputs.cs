namespace Hetzerize.Transformer;

sealed record TransformerInputs(
    string CsvPath, 
    string ?OutputPath,
    string SrcDelim, 
    string TrgDelim, 
    bool Force,
    bool Verbose);