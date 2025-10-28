public class TextDisplayAction : MenuAction
{
    public TextDisplayAction(string label = "") : base(label) { }
    public override Task<bool> ExecuteAsync(Menu menu) => Task.FromResult(true);
}
