using System.Security.Cryptography;
using System.Xml;

namespace qmlib.portfolio;

public record struct Quote(string AssetId, DateTime Date, double High, double Low, double Last, double Volume = 0.0)
{ }

public class Market(IEnumerable<Quote> quotes)
{
    private readonly ILookup<string,Quote> _market = quotes.ToLookup(x => x.AssetId);
    
    public IEnumerable<string> GetAssetIds()
    {
        return _market.Select(x => x.Key);
    }
    public Quote[] GetPrices(string assetId, DateTime startDate, DateTime endDate)
    {
        var quotes = _market[assetId].Where(x => x.Date >= startDate && x.Date <= endDate);
        return quotes.ToArray();
    }
    
}