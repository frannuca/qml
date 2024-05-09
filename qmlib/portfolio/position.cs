using Deedle;

namespace qmlib.portfolio;

public record struct Position( string AssetId, double Quantity, double Price, double Value)
{
    public Position(string assetId, double quantity, double price) : 
        this( assetId, quantity, price, quantity * price) { }
}

public readonly record struct Portfolio(IReadOnlyDictionary<string,Series<DateTime,double>> timeSeries, Position[] Positions)
{
    public IEnumerable<string> AssetIds => Positions.Select(x => x.AssetId);
    public double Value => Positions.Sum(x => x.Value);
}