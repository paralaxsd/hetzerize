using Hetzerize.Csv.Models;

namespace Hetzerize.Transformer.DocumentTransformers;

interface ICsvDocumentTransformer
{
    void Transform(CsvDocument csvDoc);
}