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