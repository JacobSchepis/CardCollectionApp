public sealed class AddCardAction : IMenuAction
{
    private readonly IScryfallClient _scryfallClient;
    private readonly ICollectionRepository _collectionRepository;
    public string Label { get; set; } = "Add Card";

    public AddCardAction(IScryfallClient scryfallClient, ICollectionRepository collectionRepository)
    {
        _scryfallClient = scryfallClient ?? throw new ArgumentNullException(nameof(scryfallClient));
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
    }

    public async Task<bool> ExecuteAsync()
    {
        Console.Clear();

        Console.WriteLine("Please enter set code");
        string setCode = Console.ReadLine()!.Trim().ToLower();

        Console.WriteLine("Enter cards one by one and press enter");

        List<int> ints = new List<int>();

        while (true)
        {
            string collectorString = Console.ReadLine()!.Trim();

            if (int.TryParse(collectorString, out int collectorNumber))
                ints.Add(collectorNumber);
            else
                break;
        }

        ScryfallClient scryfall = new ScryfallClient();

        foreach (var number in ints)
        {
            Console.WriteLine($"Fetching card for set {setCode} and collector number {number}...");

            string something = number.ToString();

            var card = await _scryfallClient.FetchBySetAndNumberAsync(setCode, something);

            if (card != null)
            {
                Console.WriteLine($"Fetched card: {card}");
                //cardRepo.AddCard(card.Name, setCode, number);
            }
            else
            {
                Console.WriteLine($"No card found for set {setCode} and collector number {number}.");
            }
        }

        string end = Console.ReadLine()!.Trim().ToLower();
        return true;
    }
}
