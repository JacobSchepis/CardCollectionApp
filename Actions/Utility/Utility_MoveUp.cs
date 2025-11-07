public class Utility_MoveUp : MenuAction
{
    public Utility_MoveUp() : base("Move Up") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.MoveCursorUp();
        return Task.FromResult(true);
    }
}
