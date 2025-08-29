using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public sealed class ViewCardsAction : IMenuAction
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly List<ICardFilter> _filters = new()
    {
        new CollectorNumberFilter()
    };

    public string Label { get; set; } = "View Cards";





    public ViewCardsAction(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
    }

    public async Task<bool> ExecuteAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Your Cards:");
            Console.WriteLine("----------");
            Console.WriteLine("help, exit");
            Console.WriteLine("> ");

            var input = Console.ReadLine()!.Trim().ToLower();

            if(input == "exit" || input == "quit")
                return true;
            else if(input == "help")
            {
                ShowHelp();
                continue;
            }

            var tokens = Regex.Matches(input, @"(\w+):(""[^""]+""|\S+)")
                .Cast<Match>()
                .GroupBy(m => m.Groups[1].Value) // group by key
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Groups[2].Value.Trim('"') // keep first occurrence only
                );

            SqlWhereBuilder whereBuilder = new SqlWhereBuilder();

            foreach(var token in tokens)
            {
                var filter = _filters.FirstOrDefault(f => f.Identifier.Equals(token.Key, StringComparison.OrdinalIgnoreCase));
                if(filter is null)
                {
                    Console.WriteLine($"Unknown filter: {token.Key}");
                    Console.ReadLine();
                    continue;
                }
                filter.ApplySql(whereBuilder, token.Value);
            }

            var where = whereBuilder.Build();
            
            var cards = _collectionRepository.GetScryfallCards(where);

            Console.Write(cards.Count());

            foreach(var card in cards)
            {
                Console.WriteLine($"{card.Name}, Set:{card.SetCode}, CN:{card.CollectorNumber}");
            }

            Console.ReadLine();
        }
    }

    private void ShowHelp()
    {
        Console.Clear();
        foreach (var filter in _filters)
        {
            Console.WriteLine($"{filter.Identifier}: {filter.HelpDescription}");
        }
        Console.ReadLine();
    }

}