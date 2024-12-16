using ConsoleAppFramework;

namespace Hetzerize.ArgumentHandling;

sealed class Commands
{
    /// <summary>
    /// Transforms a given CSV file into a new hard coded target representation.
    /// * Columns are now separated by semicolons.
    /// * Numeric values use commas as decimal separators.
    /// </summary>
    /// <param name="csvPath">
    /// Path of the input CSV file.
    /// </param>
    /// <param name="outputPath">
    /// Path of the output file. If not given, the input file directory is used
    /// in conjunction with a default file name.
    /// </param>
    /// <param name="srcDelim">
    /// The CSV delimiter of the input file.
    /// </param>
    /// <param name="trgDelim">
    /// The CSV delimiter of the output file.
    /// </param>
    /// <param name="force">
    /// If 'true', overwrites the output file if it already exists.
    /// </param>
    /// <param name="verbose">
    /// If 'true', writes extra output to the console.
    /// </param>
    [Command("")]
    public void TransformCsvData
        ([Argument] string csvPath, string? outputPath = null, string srcDelim = ",", string trgDelim = ";", bool force = false, bool verbose = false) =>
        CommandExecutor.TransformCsvData(csvPath, outputPath, srcDelim, trgDelim, force, verbose);
}