using System.Globalization;
using System.Text;
using Raiffeisen.CSV.Reader;

namespace Raiffeisen.CSV.Test;

public class TestParser
{
    private ICsvParser _parser;

    private static readonly string[] csvExample =
    {
        "06.09.2021;\"Payment reference: Card Purchase Card sequence no: 1\";03.09.2021;-5,20;EUR;04.09.2021 00:21:12:696",
        "06.09.2021;\"Purpose of use: Another Card Purchase Payment reference: POS  29,90 AT  K1  02.09. 19:31 Card sequence no: 1\";02.09.2021;-29,90;EUR;04.09.2021 07:39:10:733",
        "06.09.2021;\"Payment recipient: Company Purpose of use: Power Bill Payment recipient IBAN: ATxxxxxxxxxxxxxxxxxx Payment recipient BIC: BXXXXXXWXXX Payment recipient: Power Company Recipient ID: ATxxxxxxxxxxxxxxxxxx Client reference: 005205111110 Mandate: 000013911111, 18.11.2020\";06.09.2021;-37,00;EUR;06.09.2021 04:24:02:825",
        "01.09.2021;\"Client: Job Purpose of use: Salary 10/2021 Client IBAN: ATxxxxxxxxxxxxxxxxxx Client BIC: BXXXXXXWXXX Client reference: 00181138\";01.09.2021;2064,77;EUR;01.09.2021 04:18:51:905"
    };
    

    [SetUp]
    public void Setup()
    {
        _parser = new CsvParser();
    }

    [Test]
    public void TestCsvParser()
    {
        var csvString = string.Join(Environment.NewLine, csvExample);
        var result = _parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(csvString))).ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result[0].ValueDate, Is.EqualTo(new DateTime(2021, 9, 6)));
            Assert.That(result[0].PaymentReference, Is.EqualTo("Card Purchase"));
        });
    }
}