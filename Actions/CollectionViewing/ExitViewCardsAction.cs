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
