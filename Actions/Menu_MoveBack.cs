public class Menu_MoveBack : MenuAction
{
    public Menu_MoveBack() : base("Exit") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.PopActions();
        return Task.FromResult(true);
    }
}