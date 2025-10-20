using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public sealed class ViewCardsAction : IMenuAction
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly List<ICardFilter> _filters = new()
    {
        new CollectorNumberFilter(),
        new SetFilter(),
        new CmcFilter()
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

            Console.Clear();

            Console.WriteLine(
                    $"{"Name",-35} {"Set",-40} {"Rarity",-10} {"ManaCost",-12} {"CMC",-5} {"Price",-10} {"Foil",-10} {"Quantity", -15}");

            int maxLength = 26;

            foreach (var card in cards)
            {
                string shortString = card.SetName.Length > maxLength
                        ? card.SetName.Substring(0, maxLength) + "..."
                        : card.SetName;

                Console.WriteLine(
                    $"{card.Name,-35} {shortString,-40} {card.Rarity,-10} {card.ManaCost,-12} {card.Cmc,-5} {card.Price,-10} {card.IsFoil, -10} {card.Quantity, -15}");
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