public interface IMenuRenderer
{
    void Initialize(MenuView view, int cursor);

    void RenderSelectionChange(MenuView view, int oldCursor, int newCursor);

    void Rebuild(MenuView view, int cursor);
}

public sealed record MenuView
{
    public string Title { get; }
    public IReadOnlyList<string> Items { get; }

    public MenuView(string title, IReadOnlyList<MenuAction> actions)
    {
        Title = title;
        Items = actions.Select(a => a.Label).ToList();
    }
}
public sealed class ConsoleMenuRendererOptions
{
    public bool UseUnderline { get; set; } = false;      // ANSI underline
    public bool UseInvertColors { get; set; } = true;    // black/gray highlight
    public bool ShowPointer { get; set; } = false;       // ">" prefix
    public string PointerText { get; set; } = "> ";

    // Colors for invert mode
    public ConsoleColor SelectedFg { get; set; } = ConsoleColor.Black;
    public ConsoleColor SelectedBg { get; set; } = ConsoleColor.Gray;

    // Non-selected prefix
    public string UnselectedPrefix { get; set; } = "  ";

    // Enable ANSI if you want underline/color sequences (Windows Terminal/macOS/Linux)
    public bool EnableAnsi { get; set; } = false;
}


public sealed class ConsoleMenuRenderer : IMenuRenderer
{
    private readonly ConsoleMenuRendererOptions _opt;
    private int _startRow;   // first row of items (title is at _startRow - 1)

    public ConsoleMenuRenderer(ConsoleMenuRendererOptions opt) => _opt = opt;

    public void Initialize(MenuView view, int cursor)
    {
        Console.CursorVisible = false;

        if (_opt.EnableAnsi) TryEnableAnsi();

        Console.Clear();
        Console.WriteLine(view.Title);
        _startRow = Console.CursorTop;

        for (int i = 0; i < view.Items.Count; i++)
            WriteLineAt(_startRow + i, view.Items[i], selected: i == cursor);
    }

    public void RenderSelectionChange(MenuView view, int oldCursor, int newCursor)
    {
        if (oldCursor >= 0 && oldCursor < view.Items.Count)
            WriteLineAt(_startRow + oldCursor, view.Items[oldCursor], selected: false);
        if (newCursor >= 0 && newCursor < view.Items.Count)
            WriteLineAt(_startRow + newCursor, view.Items[newCursor], selected: true);
    }

    public void Rebuild(MenuView view, int cursor)
    {
        // full repaint (but still no Clear if you don’t want). Keeping simple:
        Console.Clear();
        Console.SetCursorPosition(0, _startRow - 1);
        ClearDown(view.Items.Count + 1); // title + items
        Console.SetCursorPosition(0, _startRow - 1);
        Console.WriteLine(view.Title);
        for (int i = 0; i < view.Items.Count; i++)
            WriteLineAt(_startRow + i, view.Items[i], selected: i == cursor);
    }

    // ---------- helpers ----------
    private void WriteLineAt(int row, string text, bool selected)
    {
        Console.SetCursorPosition(0, row);

        string prefix = selected
            ? (_opt.ShowPointer ? _opt.PointerText : string.Empty)
            : _opt.UnselectedPrefix;

        string line = prefix + text;
        int width = Math.Max(1, Console.WindowWidth);
        if (line.Length >= width) line = line[..(width - 1)];
        else line = line.PadRight(width - 1);

        if (selected)
        {
            if (_opt.UseUnderline && _opt.EnableAnsi)
                WriteAnsiUnderlined(line);
            else if (_opt.UseInvertColors)
                WriteInverted(line);
            else
                Console.Write(line);
        }
        else
        {
            Console.Write(line);
        }
    }

    private void WriteInverted(string line)
    {
        var fg = Console.ForegroundColor;
        var bg = Console.BackgroundColor;
        Console.ForegroundColor = _opt.SelectedFg;
        Console.BackgroundColor = _opt.SelectedBg;
        Console.Write(line);
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
    }

    private void WriteAnsiUnderlined(string line)
    {
        // underline on (ESC[4m), reset (ESC[0m)
        Console.Write("\u001b[4m");
        Console.Write(line);
        Console.Write("\u001b[0m");
    }

    private static void ClearDown(int lines)
    {
        int width = Math.Max(1, Console.WindowWidth);
        for (int i = 0; i < lines; i++)
        {
            Console.Write(new string(' ', width - 1));
            if (i < lines - 1) Console.WriteLine();
        }
    }

    private static void TryEnableAnsi()
    {
        // Best-effort enable VT on Windows Console (safe no-op elsewhere)
        const int STD_OUTPUT_HANDLE = -11;
        const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        try
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(handle, out int mode))
                SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
        catch { /* ignore */ }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")] private static extern IntPtr GetStdHandle(int nStdHandle);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")] private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")] private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
}
