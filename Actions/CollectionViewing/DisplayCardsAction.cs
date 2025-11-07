using System.Text.RegularExpressions;

public class DisplayCardsAction : MenuAction
{
    private const int NAME_WIDTH = -55;
    private const int SET_WIDTH = -40;
    private const int RARITY_WIDTH = -10;
    private const int MANACOST_WIDTH = -12;
    private const int CMC_WIDTH = -5;
    private const int PRICE_WIDTH = -10;
    private const int FOIL_WIDTH = -10;
    private const int QUANTITY_WIDTH = -15;

    private readonly ICollectionRepository _collectionRepository;
    private readonly List<ICardFilter> _filters;
    private readonly ViewCardsConfig _config;
    public DisplayCardsAction(ICollectionRepository collectionRepository, List<ICardFilter> filters, ViewCardsConfig config) : base("Display Cards")
    {
        _collectionRepository = collectionRepository;
        _filters = filters;
        _config = config;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        var tokens = Regex.Matches(string.Empty, @"(\w+):(""[^""]+""|\S+)")
                .Cast<Match>()
                .GroupBy(m => m.Groups[1].Value) // group by key
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Groups[2].Value.Trim('"') // keep first occurrence only
                );

        SqlWhereBuilder whereBuilder = new SqlWhereBuilder();

        foreach (var token in tokens)
        {
            var filter = _filters.FirstOrDefault(f => f.Identifier.Equals(token.Key, StringComparison.OrdinalIgnoreCase));
            if (filter is null)
            {
                Console.WriteLine($"Unknown filter: {token.Key}");
                Console.ReadLine();
                continue;
            }
            filter.ApplySql(whereBuilder, token.Value);
        }

        var where = whereBuilder.Build();

        var cards = _collectionRepository.GetScryfallCards(where, _config.pageSize, _config.currentPage);

        Console.Clear();

        string header =
            $"{"Name",NAME_WIDTH} " +
            $"{"Set",SET_WIDTH} " +
            $"{"Rarity",RARITY_WIDTH} " +
            $"{"ManaCost",MANACOST_WIDTH} " +
            $"{"CMC",CMC_WIDTH} " +
            $"{"Price",PRICE_WIDTH} " +
            $"{"Foil",FOIL_WIDTH} " +
            $"{"Quantity",QUANTITY_WIDTH}";

        //Console.WriteLine(header);

        int maxLength = 26;

        var actionList = new List<MenuAction>();

        foreach (var card in cards)
        {
            string shortString = card.SetName.Length > maxLength
                    ? card.SetName.Substring(0, maxLength) + "..."
                    : card.SetName;

            string line =
                $"{card.Name,NAME_WIDTH} " +
                $"{shortString,SET_WIDTH} " +
                $"{card.Rarity,RARITY_WIDTH} " +
                $"{card.ManaCost,MANACOST_WIDTH} " +
                $"{card.Cmc,CMC_WIDTH} " +
                $"{card.Price,PRICE_WIDTH} " +
                $"{card.IsFoil,FOIL_WIDTH} " +
                $"{card.Quantity,QUANTITY_WIDTH}";

            actionList.Add(new TempMenu(line, _config, this, (card.GetUniqueKey()), _collectionRepository));
        }

        actionList.Add(new Utility_TextDisplayAction());
        actionList.Add(new Utility_MoveBack());

        menu.PushActions(actionList);
        menu.AddActionMapping(ConsoleKey.RightArrow, new NextPageAction(_config, this));
        menu.AddActionMapping(ConsoleKey.LeftArrow, new PreviousPageAction(_config, this));

        return Task.FromResult(true);
    }
}
