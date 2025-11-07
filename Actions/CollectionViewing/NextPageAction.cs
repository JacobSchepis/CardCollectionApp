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
