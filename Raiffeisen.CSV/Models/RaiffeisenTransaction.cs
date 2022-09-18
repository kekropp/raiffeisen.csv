using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Raiffeisen.CSV.Models;

public class RaiffeisenTransaction
{
    public DateTime ValueDate { get; set; }
    public DateTime TransactionDate { get; set; }
    
    [Marker(Marker.Client)]
    [MarkerString("Client: ")]
    public string? Client { get; set; }

    [Marker(Marker.ClientIban)]
    [MarkerString("Client IBAN: ")]
    public string? ClientIban { get; set; }

    [Marker(Marker.ClientBic)]
    [MarkerString("Client BIC: ")]
    public string? ClientBic { get; set; }
    
    [Marker(Marker.ClientReference)]
    [MarkerString("Client reference: ")]
    public string? ClientReference { get; set; }

    [Marker(Marker.Recipient)]
    [MarkerString("Payment recipient: ")]
    public string? Recipient { get; set; }

    [Marker(Marker.RecipientIban)]
    [MarkerString("Payment recipient IBAN: ")]
    public string? RecipientIban { get; set; }

    [Marker(Marker.RecipientBic)]
    [MarkerString("Payment recipient BIC: ")]
    public string? RecipientBic { get; set; }

    [Marker(Marker.RecipientId)]
    [MarkerString("Recipient ID: ")]
    public string? RecipientId { get; set; }
    
    [Marker(Marker.PurposeOfUse)]
    [MarkerString("Purpose of use: ")]
    public string? PurposeOfUse { get; set; }

    [Marker(Marker.PaymentReference)]
    [MarkerString("Payment reference: ")]
    public string? PaymentReference { get; set; }

    [Marker(Marker.CardSequenceNumber)]
    [MarkerString("Card sequence no: ")]
    [ValueIsInteger]
    public int? CardSequenceNumber { get; set; }
    
    [Marker(Marker.Mandate)]
    [MarkerString("Mandate: ")]
    public string? Mandate { get; set; }
    public double Amount { get; set; }
    public string? CurrencyCode { get; set; }

    public static RaiffeisenTransaction FromFields(string[]? fields)
    {
        if (fields == null)
            throw new InvalidDataException("Cannot add transaction without fields");

        var tx = new RaiffeisenTransaction
        {
            ValueDate = DateTime.ParseExact(fields[0], "dd.MM.yyyy", CultureInfo.InvariantCulture),
            Amount = Convert.ToDouble(fields[3].Replace(",", "."), CultureInfo.InvariantCulture),
            CurrencyCode = fields[4],
            TransactionDate = DateTime.ParseExact(fields[5], "dd.MM.yyyy hh:mm:ss:fff", CultureInfo.InvariantCulture),
        };

        var markers = FindMarkers(fields[1]);
        var parts = SplitByMarkers(fields[1], markers);

        var allProperties = typeof(RaiffeisenTransaction).GetProperties();
        var mapping = allProperties.Select(x => (property: x, marker: x.GetCustomAttribute(typeof(MarkerAttribute))))
            .Where(x => x.marker != null)
            .Select(x => (x.property, marker: ((MarkerAttribute)x.marker!).Marker));

        foreach (var part in parts)
        {
            var entry = mapping.FirstOrDefault(x => x.marker == part.Key);
            if (entry == default((PropertyInfo, Marker)))
                continue;
            entry.property.SetValue(tx, entry.marker.ConvertValue(part.Value));
        }

        return tx;
    }

    private static Dictionary<Marker, int> FindMarkers(string content)
    {
        return Enum.GetValues<Marker>()
            .Select(x => (type: x, location: x.TryFindInString(content)))
            .Where(x => x.location.HasValue)
            .ToDictionary(x => x.type, y => y.location!.Value);
    }

    private static Dictionary<Marker, string> SplitByMarkers(string content, Dictionary<Marker, int> markers)
    {
        return markers.ToDictionary(
            x => x.Key,
            y => content.Substring(y.Value + y.Key.GetMarkerLength(),
                markers.Values.Where(z => z > y.Value)
                    .OrderBy(z => z).DefaultIfEmpty(content.Length).Min() - y.Key.GetMarkerLength() - y.Value).Trim()
        );
    }
}

internal enum Marker
{
    Client,
    ClientIban,
    ClientBic,
    Recipient,
    RecipientIban,
    RecipientBic,
    RecipientId,
    PurposeOfUse,
    PaymentReference,
    CardSequenceNumber,
    ClientReference,
    Mandate
}

#region Attributes

[AttributeUsage(AttributeTargets.Property)]
internal class MarkerAttribute : Attribute
{
    public Marker Marker { get; set; }

    public MarkerAttribute(Marker type)
    {
        Marker = type;
    }
}

[AttributeUsage(AttributeTargets.Property)]
internal class ValueIsInteger : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class ValueIsDouble : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
internal class MarkerStringAttribute : Attribute
{
    public string MarkerString { get; set; }

    public MarkerStringAttribute(string markerString)
    {
        MarkerString = markerString;
    }
}

#endregion

internal static class MarkerTypeExtension
{
    public static int? TryFindInString(this Marker marker, string content)
    {
        var sIdx = content.IndexOf(marker.GetMarkerString(), 0, StringComparison.InvariantCultureIgnoreCase);
        return sIdx == -1 ? null : sIdx;
    }

    public static int GetMarkerLength(this Marker marker)
    {
        return marker.GetMarkerString().Length;
    }

    private static string GetMarkerString(this Marker marker)
    {
        var properties = typeof(RaiffeisenTransaction).GetProperties();
        var markerString = properties
            .Select(x => (property: x, marker: x.GetCustomAttribute(typeof(MarkerStringAttribute))))
            .Where(x => x.marker != null)
            .Select(x => (x.property, marker: ((MarkerStringAttribute)x.marker!).MarkerString))
            .FirstOrDefault(x =>
                x.property.GetCustomAttribute(typeof(MarkerAttribute)) is MarkerAttribute ma && ma.Marker == marker);
        return markerString.marker;
    }

    public static object ConvertValue(this Marker marker, string extractedContent)
    {
        var properties = typeof(RaiffeisenTransaction).GetProperties();
        var property = properties.Select(x => (property: x, marker: x.GetCustomAttribute(typeof(MarkerAttribute))))
            .Where(x => x.marker != null)
            .Select(x => (x.property, marker: ((MarkerAttribute)x.marker!).Marker))
            .FirstOrDefault(x => x.marker == marker).property;
        if (Attribute.GetCustomAttribute(property, typeof(ValueIsInteger)) != null)
            return Convert.ToInt32(extractedContent);
        if (Attribute.GetCustomAttribute(property, typeof(ValueIsDouble)) != null)
            return Convert.ToDouble(extractedContent, CultureInfo.InvariantCulture);
        return extractedContent;
    }
}