public class Menu_MoveDown : MenuAction
{
    public Menu_MoveDown() : base("Move Down") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.MoveCursorDown();
        return Task.FromResult(true);
    }
}
