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
