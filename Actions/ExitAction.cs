public sealed class ExitAction : IMenuAction
{
    public string Label { get; set; } = "Exit";

    public Task<bool> ExecuteAsync()
    {
        return Task.FromResult(false);
    }
}
