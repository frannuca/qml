namespace qmlib.portfolio;

public record struct Position( string AssetId, double Quantity, double Price, double Value)
{
    public Position(string assetId, double quantity, double price) : 
        this( assetId, quantity, price, quantity * price) { }
}

public readonly record struct Portfolio(DateTime Date, Position[] Positions)
{
    public IEnumerable<string> AssetIds => Positions.Select(x => x.AssetId);
    public double Value => Positions.Sum(x => x.Value);
}