public interface IMenuAction
{
    string Label { get; protected set; }
    Task<bool> ExecuteAsync();
}
