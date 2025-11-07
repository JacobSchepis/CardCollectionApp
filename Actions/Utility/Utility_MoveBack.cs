public class Utility_MoveBack : MenuAction
{
    public Utility_MoveBack() : base("Exit") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.PopActions();
        return Task.FromResult(true);
    }
}