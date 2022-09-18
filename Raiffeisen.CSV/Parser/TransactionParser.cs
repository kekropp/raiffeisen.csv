using Microsoft.VisualBasic.FileIO;
using Raiffeisen.CSV.Models;

namespace Raiffeisen.CSV.Parser;

public class TransactionParser : ITransactionParser
{
    public  IEnumerable<RaiffeisenTransaction> Parse(Stream data)
    {
        using var parser = new TextFieldParser(data)
        {
            Delimiters = new[] { ";" },
            HasFieldsEnclosedInQuotes = true,
            TextFieldType = FieldType.Delimited,
            TrimWhiteSpace = true
        };

        while (!parser.EndOfData)
        {
            yield return RaiffeisenTransaction.FromFields(parser.ReadFields());
        }
        
    }
}