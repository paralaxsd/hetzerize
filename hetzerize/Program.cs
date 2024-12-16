using ConsoleAppFramework;
using static Hetzerize.CommandExecutor;

namespace Hetzerize;

static class Program
{
    static void Main(string[] args) =>
        ConsoleApp.Run(args,
            ([Argument] string csvPath, string srcDelim = ",", string trgDelim = ";", bool force = false) =>
                TransformCsvData(csvPath, srcDelim, trgDelim, force));
}