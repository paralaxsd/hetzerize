using Hetzerize.Transformer;

namespace Hetzerize;

static class CommandExecutor
{
    public static int TransformCsvData(string csvPath, string srcDelim, string trgDelim, bool force)
    {
        var inputs = new TransformerInputs(csvPath, srcDelim, trgDelim, force);
        return new CsvTransformer(inputs).Execute();
    }
}