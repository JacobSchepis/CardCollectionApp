public abstract class MenuAction
{
    protected MenuAction(string label)
    {
        Label = label;
    }

    public string Label { get; private set; }
    public abstract Task<bool> ExecuteAsync(Menu menu);
}
