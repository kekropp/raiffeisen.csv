
using Raiffeisen.CSV.Models;

namespace Raiffeisen.CSV.Parser;

public interface ITransactionParser
{
    public IEnumerable<RaiffeisenTransaction> Parse(Stream data);
}