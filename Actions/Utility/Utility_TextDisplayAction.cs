public class Utility_TextDisplayAction : MenuAction
{
    public Utility_TextDisplayAction(string label = "") : base(label) { }
    public override Task<bool> ExecuteAsync(Menu menu) => Task.FromResult(true);
}
