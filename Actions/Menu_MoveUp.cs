
public class Menu_MoveUp : MenuAction
{
    public Menu_MoveUp() : base("Move Up") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.MoveCursorUp();
        return Task.FromResult(true);
    }
}
