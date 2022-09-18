
using Raiffeisen.CSV.Models;

namespace Raiffeisen.CSV.Reader;

public interface ICsvParser
{
    public IEnumerable<RaiffeisenTransaction> Parse(Stream data);
}