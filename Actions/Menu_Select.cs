public class Menu_Select : MenuAction
{
    public Menu_Select() : base("Select") { }
    public override async Task<bool> ExecuteAsync(Menu menu)
    {
        return await menu._actionStack.Peek()[menu._cursor].ExecuteAsync(menu);
    }
}
