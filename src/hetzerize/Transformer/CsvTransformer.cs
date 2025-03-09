using Hetzerize.Csv;
using System.ComponentModel.DataAnnotations;
using Spectre.Console;
using Hetzerize.Extensions;
using Hetzerize.Csv.Models;
using Hetzerize.Transformer.DocumentTransformers;

namespace Hetzerize.Transformer;

sealed class CsvTransformer(TransformerInputs inputs)
{
    /******************************************************************************************
     * FIELDS
     * ***************************************************************************************/
    readonly FileInfo _csvFile = new(inputs.CsvPath);
    readonly string _srcDelim = inputs.SrcDelim;
    readonly string _trgDelim = inputs.TrgDelim;
    readonly bool _force = inputs.Force;
    readonly bool _verbose = inputs.Verbose;

    string? _outputPath = inputs.OutputPath;

    const string Banner = 
        """
            )
         ( /(           )
         )\())   (   ( /(       (   (   (         (
        ((_)\   ))\  )\())(    ))\  )(  )\  (    ))\
         _((_) /((_)(_))/ )\  /((_)(()\((_) )\  /((_)
        | || |(_))  | |_ ((_)(_))   ((_)(_)((_)(_))
        | __ |/ -_) |  _||_ // -_) | '_|| ||_ // -_)
        |_||_|\___|  \__|/__|\___| |_|  |_|/__|\___|
                          
        """;


    /******************************************************************************************
     * PROPERTIES
     * ***************************************************************************************/
    FileInfo OutputFile => new(_outputPath ??= CreateDefaultOutputPath());

    IReadOnlyList<ICsvDocumentTransformer> DocTransformers {get;} =
    [
        // Replacements:
        new GermanDateColumnTransformer("Booking Date", "Zahlungsdatum"),
        new GermanDateColumnTransformer("Value Date", "Buchungsdatum"),
        new RenameColumnTransformer("Partner Name", "Auftraggeber"),
        new RenameColumnTransformer("Partner Iban", "IBAN"),
        new RemoveColumnTransformer("Type"),
        new RenameColumnTransformer("Payment Reference", "Verwendungszweck"),
        new RemoveColumnTransformer("Account Name"),
        new GermanNumberColumnTransformer("Amount (EUR)", "Betrag"),
        new RemoveColumnTransformer("Original Amount"),
        new RemoveColumnTransformer("Original Currency"),
        new RemoveColumnTransformer("Exchange Rate"),
        // Insertions:
        new InsertConstantDataColumnTransformer("Belegnr", 2, "1"),
        // Moves:
        new MoveColumnTransformer("IBAN", 5),
        new MoveColumnTransformer("Betrag", 3),
        // Post-processing:
        new UnquoteColumnTransformer("Auftraggeber"),
        new UnquoteColumnTransformer("Verwendungszweck"),
    ];

    /******************************************************************************************
     * METHODS
     * ***************************************************************************************/
    public void Execute()
    {
        PrintBanner();
        EnsurePreconditionsAreMet();

        try
        {
            var csvDoc = ReadCsvDocument();
            PerformTransformationsOn(csvDoc);
            WriteTransformedCsvDocument(csvDoc);

            AnsiConsole.MarkupLine($":check_mark_button: [green]Output written to [teal link]{OutputFile.FullName}[/].[/]");
        }
        catch (Exception e)
        {
            PrintException(e);
        }
    }

    void PrintException(Exception e)
    {
        AnsiConsole.MarkupLine(":skull: [red]Transforming input data failed:[/]");
        if(_verbose)
        {
            AnsiConsole.WriteException(e);
        }
        else
        {
            AnsiConsole.Foreground = Color.Red;
            AnsiConsole.WriteLine(Markup.Escape(e.Message));
            AnsiConsole.ResetColors();
        }
    }

    void EnsurePreconditionsAreMet()
    {
        if (!_csvFile.Exists)
        {
            throw new ValidationException($"File '{_csvFile.FullName}' not found.");
        }

        if (OutputFile.Exists)
        {
            if(_force)
            {
                if(_verbose)
                {
                    AnsiConsole.MarkupLine($"[cyan]Overwriting {OutputFile.Name} as per user request.[/]");
                }
            }
            else
            {
                throw new ValidationException($"The output file '{OutputFile.FullName}' " + 
                    $"already exists. Use --force to overwrite it.");
            }
        }
    }

    CsvDocument ReadCsvDocument()
    {
        using var streamReader = _csvFile.OpenText();
        return new CsvReader(_srcDelim).ReadFrom(streamReader);
    }

    void WriteTransformedCsvDocument(CsvDocument csvDoc)
    {
        var csvWriter = new CsvWriter(_trgDelim);
        using var streamWriter = OutputFile.CreateText();
        csvWriter.WriteTo(streamWriter, csvDoc);
    }

    string CreateDefaultOutputPath()
    {
        var now = DateTime.Now;
        var prefix = $"{now.Year % 1000}{now.Month:D2}{now.Day:D2}";
        var filename = $"{prefix}_statement.csv";

        return Path.Join(_csvFile.Directory!.FullName, filename);
    }

    void PerformTransformationsOn(CsvDocument csvDoc) => DocTransformers
        .Apply(docTrans => docTrans.Transform(csvDoc));

    static void PrintBanner() => AnsiConsole.MarkupLine($"[yellow]{Banner}[/]");
}
