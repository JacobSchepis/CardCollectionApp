using Microsoft.Data.Sqlite;


class Program
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")] static extern IntPtr GetStdHandle(int nStdHandle);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")] static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")] static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

    public static async Task Main()
    {
        Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        EnableAnsi();

        CardRepository repo = new CardRepository("Data Source=cards.db");
        repo.EnsureSchema();

        Menu menu = new Menu("Main Menu", new List<MenuAction>
        {
            new AddCardAction(new ScryfallClient(), repo),
            new ViewCardsAction(repo),
            new ExitApplicationAction()
        });
        await menu.DisplayMenu();
        
    }

    static void EnableAnsi()
    {
        const int STD_OUTPUT_HANDLE = -11;
        const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        var handle = GetStdHandle(STD_OUTPUT_HANDLE);
        if (GetConsoleMode(handle, out int mode))
            SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }
}