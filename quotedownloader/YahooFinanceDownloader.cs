using Deedle;
using YahooFinanceApi;

namespace qml.quoteDownloader;

public class YahooFinanceDownloader
{
    public static async Task<Series<DateTime, double>> DownloadTimeSeriesData(string stockSymbol, DateTime startDate,
        DateTime endDate)
    {
        var r = await Yahoo.GetHistoricalAsync(stockSymbol, startDate, endDate);
        var b = r
            .Select(a => new KeyValuePair<DateTime, double>(a.DateTime, (double)a.Close))
            .Where(kv => kv.Value > 0);
        return new Series<DateTime, double>(b);
    }
}