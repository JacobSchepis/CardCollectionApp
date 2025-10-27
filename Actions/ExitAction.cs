public sealed class ExitAction : MenuAction
{
    public ExitAction() : base("Exit") {}

    public override Task<bool> ExecuteAsync(Menu menu)
    {
        return Task.FromResult(false);
    }
}
