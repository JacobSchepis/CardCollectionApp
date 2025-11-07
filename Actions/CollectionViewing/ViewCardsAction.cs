using CardCollectionApp.Filters;
using System.Diagnostics;
using System.Text;

public sealed class ViewCardsAction : MenuAction
{
    private readonly List<ICardFilter> _filters = new()
    {
        new CollectorNumberFilter(),
        new SetFilter(),
        new CmcFilter()
    };

    private readonly List<MenuAction> _actions;
    private ViewCardsConfig _config = new();

    public ViewCardsAction(ICollectionRepository collectionRepository) : base("View Cards")
    {
        _actions = new()
        {
            new DisplayCardsAction(collectionRepository, _filters, _config),
            new ExitViewCardsAction()
        };
    }

    public override Task<bool> ExecuteAsync(Menu menu)
    {
        _config.currentPage = 0;
        menu.PushActions(_actions);

        return Task.FromResult(true);
    }
}

public class ViewCardsConfig
{
    public int currentPage = 0;
    public int pageSize = 30;


}

