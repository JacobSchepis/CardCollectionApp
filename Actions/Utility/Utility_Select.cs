public class Utility_Select : MenuAction
{
    public Utility_Select() : base("Select") { }
    public override async Task<bool> ExecuteAsync(Menu menu)
    {
        return await menu._actionStack.Peek()[menu._cursor].ExecuteAsync(menu);
    }
}
