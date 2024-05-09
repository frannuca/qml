using YahooFinance.NET;
using YahooFinanceApi;

namespace qml.quoteDownloader;

using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System;
using YahooFinance.NET;
using Deedle;

public class YahooFinanceDownloader
{
   

    
    public static async Task<Series<DateTime,double>> DownloadTimeSeriesData(string stockSymbol, DateTime startDate, DateTime endDate)
    {
        var r  = await Yahoo.GetHistoricalAsync(stockSymbol, startDate, endDate, Period.Daily);
        var b = r
            .Select(a => new KeyValuePair<DateTime, double>(a.DateTime, (double)a.Close))
            .Where(kv => kv.Value > 0);
        return new Series<DateTime, double>(b);

    }
}
