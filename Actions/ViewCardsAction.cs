using CardCollectionApp.Filters;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public sealed class ViewCardsAction : MenuAction
{
    private readonly List<ICardFilter> _filters = new()
    {
        new CollectorNumberFilter(),
        new SetFilter(),
        new CmcFilter()
    };

    private readonly List<MenuAction> _actions;
    private ViewCardsConfig _config = new();

    public ViewCardsAction(ICollectionRepository collectionRepository) : base("View Cards")
    {
        _actions = new()
        {
            new DisplayCardsAction(collectionRepository, _filters, _config),
            new ExitViewCardsAction()
        };
    }

    public override Task<bool> ExecuteAsync(Menu menu)
    {
        _config.currentPage = 0;
        menu.PushActions(_actions);

        return Task.FromResult(true);
    }
}

public class ViewCardsConfig
{
    public int currentPage = 0;
    public int pageSize = 30;
}

public class ExitViewCardsAction : MenuAction
{
    public ExitViewCardsAction() : base("Exit View Cards") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.PopActions();
        menu.RemoveActionMapping(ConsoleKey.RightArrow);
        menu.RemoveActionMapping(ConsoleKey.LeftArrow);
        return Task.FromResult(true);
    }
}

public class ShowHelpMenu : MenuAction
{
    private readonly List<ICardFilter> _filters;
    public ShowHelpMenu(List<ICardFilter> filters) : base("Show Help")
    {
        _filters = filters;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        Console.Clear();
        foreach (var filter in _filters)
        {
            Console.WriteLine($"{filter.Identifier}: {filter.HelpDescription}");
        }
        Console.ReadLine();
        return Task.FromResult(true);
    }
}

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

        actionList.Add(new TextDisplayAction());
        actionList.Add(new Menu_MoveBack());

        menu.PushActions(actionList);
        menu.AddActionMapping(ConsoleKey.RightArrow, new NextPageAction(_config, this));
        menu.AddActionMapping(ConsoleKey.LeftArrow, new PreviousPageAction(_config, this));

        return Task.FromResult(true);
    }
}

public class NextPageAction : MenuAction
{
    private readonly ViewCardsConfig _config;
    private readonly MenuAction _display;
    public NextPageAction(ViewCardsConfig config, MenuAction display) : base("Next Page")
    {
        _config = config;
        _display = display;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        _config.currentPage++;
        menu.PopActions();
        _display.ExecuteAsync(menu);
        return Task.FromResult(true);
    }
}

public class PreviousPageAction : MenuAction
{
    private readonly ViewCardsConfig _config;
    private readonly MenuAction _display;
    public PreviousPageAction(ViewCardsConfig config, MenuAction display) : base("Previous Page")
    {
        _config = config;
        _display = display;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        if (_config.currentPage > 0)
            _config.currentPage--;
        menu.PopActions();
        _display.ExecuteAsync(menu);
        return Task.FromResult(true);
    }
}

public class TextDisplayAction : MenuAction
{
    public TextDisplayAction(string label = "") : base(label) { }
    public override Task<bool> ExecuteAsync(Menu menu) => Task.FromResult(true);
}

public class TempMenu : MenuAction
{
    private readonly ViewCardsConfig _config;
    private readonly MenuAction _display;
    private readonly (string setCode, int collectorNum, bool isFoil) _id;
    private readonly ICollectionRepository _collectionRepository;
    public TempMenu(string label, ViewCardsConfig config, MenuAction displayAction, (string setCode, int collectorNum, bool isFoil) id, ICollectionRepository collectionRepository) : base(label)
    {
        _config = config;
        _display = displayAction;
        _id = id;
        _collectionRepository = collectionRepository;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        List<MenuAction> actions = new()
        {
            new TextDisplayAction("This is a test action"),
            new RemoveCardAction(_collectionRepository, _id, _display),
            new TextDisplayAction(),
            new ExitViewSingleAction(_config, _display)
        };

        menu.PushActions(actions);

        menu.RemoveActionMapping(ConsoleKey.LeftArrow);
        menu.RemoveActionMapping(ConsoleKey.RightArrow);

        return Task.FromResult(true);
    }
}

public class ExitViewSingleAction : MenuAction
{
    private readonly ViewCardsConfig _config;
    private readonly MenuAction _display;
    public ExitViewSingleAction(ViewCardsConfig config, MenuAction display) : base("Exit View Single Card") 
    {
        _config = config;
        _display = display;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.PopActions();
        
        menu.AddActionMapping(ConsoleKey.RightArrow, new NextPageAction(_config, _display));
        menu.AddActionMapping(ConsoleKey.LeftArrow, new PreviousPageAction(_config, _display));

        return Task.FromResult(true);
    }
}

public class RemoveCardAction : MenuAction
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly (string setCode, int collectorNum, bool isFoil) _id;
    private readonly MenuAction _display;
    public RemoveCardAction(ICollectionRepository collectionRepository, (string setCode, int collectorNum, bool isFoil) id, MenuAction display) : base("Remove Card")
    {
        _collectionRepository = collectionRepository;
        _id = id;
        _display = display;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        Console.Clear();


        _collectionRepository.DecrementQuantityOrDeleteAsync(_id.setCode, _id.collectorNum, _id.isFoil);

        Console.WriteLine("Card removed. Press Enter to continue.");

        Console.ReadLine();

        menu.PopActions();
        menu.PopActions();
        _display.ExecuteAsync(menu);

        return Task.FromResult(true);
    }
}