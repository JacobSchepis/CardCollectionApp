public class ShowHelpMenu : MenuAction
{
    private readonly List<ICardFilter> _filters;
    public ShowHelpMenu(List<ICardFilter> filters) : base("Show Help")
    {
        _filters = filters;
    }
    public override Task<bool> ExecuteAsync(Menu menu)
    {
        Console.Clear();
        foreach (var filter in _filters)
        {
            Console.WriteLine($"{filter.Identifier}: {filter.HelpDescription}");
        }
        Console.ReadLine();
        return Task.FromResult(true);
    }
}
