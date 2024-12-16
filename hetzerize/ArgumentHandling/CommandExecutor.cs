using Hetzerize.Transformer;

namespace Hetzerize.ArgumentHandling;

static class CommandExecutor
{
    public static void TransformCsvData(string csvPath, string? outputPath, string srcDelim, string trgDelim, bool force, bool verbose)
    {
        var inputs = new TransformerInputs(csvPath, outputPath, srcDelim, trgDelim, force, verbose);
        new CsvTransformer(inputs).Execute();
    }
}