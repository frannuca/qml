using System.Runtime.InteropServices.JavaScript;
using Accord;
using Deedle;
using qmlib.signal;

namespace qmlib.portfolio;

public abstract class Strategy
{ public abstract Portfolio MoveNext(Market market, Portfolio portfolio, int timeWindow);
}

public class CmaStrategy(IEnumerable<(double lowfreq, double highfreq, double weight)> filters)
    : Strategy
{
    private readonly IEnumerable<(double lowfreq, double highfreq, double weight)> _filters = filters;

    public override Portfolio MoveNext(Market market, Portfolio portfolio, int timeWindow)
    {
        var assetIds = portfolio.AssetIds;
        var newPositions = new List<Position>();
        foreach (var assetId in assetIds)
        {
            var quotes = market.GetPrices(assetId, portfolio.Date.AddDays(-timeWindow), portfolio.Date);
            var values = quotes.Select(x => new KeyValuePair<DateTime, double>(x.Date,x.Last)).ToArray();
            var series = new Series<DateTime, double>(values);
            var signal = series * 0.0;
            foreach (var (lowfreq, highfreq, weight) in _filters)
            {
                signal += weight * BandPassFilter.Filter(series, lowfreq, highfreq, 1.0, 5);
            }
            newPositions.Add(new Position(assetId,signal.LastValue(),double.NaN));
        }
        return new Portfolio(portfolio.Date.AddDays(1), newPositions.ToArray());
    }
}
