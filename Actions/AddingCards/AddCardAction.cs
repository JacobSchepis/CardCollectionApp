public sealed class AddCardAction : MenuAction
{
    private readonly IScryfallClient _scryfallClient;
    private readonly ICollectionRepository _collectionRepository;

    public AddCardAction(IScryfallClient scryfallClient, ICollectionRepository collectionRepository) :base("Add Card")
    {
        _scryfallClient = scryfallClient ?? throw new ArgumentNullException(nameof(scryfallClient));
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
    }

    public override async Task<bool> ExecuteAsync(Menu menu)
    {
        Console.Clear();

        Console.WriteLine("Please enter set code");
        string setCode = Console.ReadLine()!.Trim().ToLower();

        Console.WriteLine("Enter cards one by one and press enter");

        List<string> codes = new List<string>();

        while (true)
        {
            string collectorString = Console.ReadLine()!.Trim();

            if (collectorString.Equals("done", StringComparison.OrdinalIgnoreCase))
                break;

            codes.Add(collectorString);
        }

        ScryfallClient scryfall = new ScryfallClient();

        foreach (var code in codes)
        {
            if (!TryParseCollectorInput(code, out int number, out bool isFoil, out bool isPromo))
            {
                Console.WriteLine($"Invalid collector number input: {code}");
                continue;
            }

            var card = await _scryfallClient.FetchBySetAndNumberAsync(setCode, number.ToString());

            if (card == null)
            {
                Console.WriteLine($"No card found for set {setCode} and collector number {code}.");
                continue;
            }

            var cardRecord = new CardRecord(card, isFoil, isPromo);

            if (cardRecord.Price >= 20.0f)
            {
                Console.WriteLine($"Adding high-value card: {cardRecord.Name} ({cardRecord.SetCode} #{cardRecord.CollectorNumber}) - ${cardRecord.Price}");
            }
            else
            {
                Console.WriteLine($"Adding card: {cardRecord.Name} ({cardRecord.SetCode} #{cardRecord.CollectorNumber}) - ${cardRecord.Price}");
            }

            await _collectionRepository.AddAsync(cardRecord);
        }

        string end = Console.ReadLine()!;

        menu.RefreshAll();

        return true;
    }

    private static bool TryParseCollectorInput(string input, out int number, out bool isFoil, out bool isPromo)
    {
        number = 0;
        isFoil = false;
        isPromo = false;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Normalize spaces and case
        var s = input.Trim().ToLowerInvariant();

        // Collect flags anywhere in the string
        isFoil = s.Contains('+');
        isPromo = s.Contains('-');

        // Remove flag chars to get the raw collector number (can be alphanumeric in Scryfall, e.g., "123a")
        s = s.Replace("+", "").Replace("-", "").Trim();

        int parsedNumber;

        if (int.TryParse(s, out parsedNumber))
            number = parsedNumber;
        else
            return false;

        // Collector numbers can include leading zeros and letters (e.g., "045", "123a")
        number = parsedNumber;
        return true;
    }
}
