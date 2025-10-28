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
