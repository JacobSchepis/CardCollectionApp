using Microsoft.Data.Sqlite;

class Program
{
    
    public static async Task Main()
    {
        CardRepository repo = new CardRepository("Data Source=cards.db");
        repo.EnsureSchema();

        Menu menu = new Menu("Main Menu", new List<IMenuAction>
        {
            new AddCardAction(new ScryfallClient(), repo),
            new ExitAction()
        });
        await menu.DisplayMenu();





        
    }
}