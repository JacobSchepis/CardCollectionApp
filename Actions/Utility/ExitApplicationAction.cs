public sealed class ExitApplicationAction : MenuAction
{
    public ExitApplicationAction() : base("Exit") {}

    public override Task<bool> ExecuteAsync(Menu menu)
    {
        return Task.FromResult(false);
    }
}
