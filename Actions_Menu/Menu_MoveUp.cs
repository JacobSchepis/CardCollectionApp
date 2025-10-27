
public class Menu_MoveUp : MenuAction
{
    public Menu_MoveUp() : base("Move Up") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.MoveCursorUp();
        return Task.FromResult(true);
    }
}

public class Menu_MoveDown : MenuAction
{
    public Menu_MoveDown() : base("Move Down") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.MoveCursorDown();
        return Task.FromResult(true);
    }
}

public class Menu_Select : MenuAction
{
    public Menu_Select() : base("Select") { }
    public override async Task<bool> ExecuteAsync(Menu menu)
    {
        return await menu._actionStack.Peek()[menu._cursor].ExecuteAsync(menu);
    }
}

public class Menu_MoveBack : MenuAction
{
    public Menu_MoveBack() : base("Exit") { }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        menu.PopActions();
        return Task.FromResult(true);
    }
}