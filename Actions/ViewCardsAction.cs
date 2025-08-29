public enum SortField { Name, Set, CollectorNumber, ManaValue, Rarity }
public enum SortOrder { Asc, Desc }

public sealed class QueryOptions
{
    public string RawQuery { get; init; } = "";
    public SortField SortBy { get; init; } = SortField.Name;
    public SortOrder Order { get; init; } = SortOrder.Asc;
    public int PageSize { get; init; } = 50;
}

public interface ICardFilter
{
    string Label { get; }                    // short label for chips/badges
    string Describe();                       // human description for “Display”
    IEnumerable<CardRecord> ApplyInMemory(IEnumerable<CardRecord> src); // LINQ
    void ApplySql(SqlWhereBuilder b);        // push WHERE + params into builder
    bool RequiresRemote { get; }             // true => needs Scryfall-intersect
}

public sealed class ViewCardsAction : IMenuAction
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly List<ICardFilter> _filters = new();

    public string Label
    {
        get => _filters.Count == 0 ? "All cards" : string.Join(" & ", _filters.Select(f => f.Label));
        set { }
    }

    public SortField SortBy { get; private set; } = SortField.Name;
    public SortOrder Order { get; private set; } = SortOrder.Asc;
    public int PageSize { get; private set; } = 50;
    public int PageIndex { get; private set; } = 0;

    public ViewCardsAction(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
    }

    public void Display()
    {
        Console.WriteLine(Label);
        foreach (var f in _filters) Console.WriteLine(" - " + f.Describe());
        Console.WriteLine($"Sort: {SortBy} {Order}, Page: {PageIndex + 1}, Size: {PageSize}");
    }

    public async Task<bool> ExecuteAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Your Cards:");
            Console.WriteLine("----------");
            Console.WriteLine("Enter a filter, or type: help | all | exit");
            Console.WriteLine("> ");

            var input = Console.ReadLine()!.Trim().ToLower();

            

        }
    }

}