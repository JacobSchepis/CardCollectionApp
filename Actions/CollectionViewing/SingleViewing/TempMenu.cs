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
            new Utility_TextDisplayAction("This is a test action"),
            new RemoveCardAction(_collectionRepository, _id, _display),
            new Utility_TextDisplayAction(),
            new ExitViewSingleAction(_config, _display)
        };

        menu.PushActions(actions);

        menu.RemoveActionMapping(ConsoleKey.LeftArrow);
        menu.RemoveActionMapping(ConsoleKey.RightArrow);

        return Task.FromResult(true);
    }
}
