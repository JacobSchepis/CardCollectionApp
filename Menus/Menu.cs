using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Menu
{
    private string _title = "Main Menu";

    private int _cursor = 0;

    private readonly List<IMenuAction> _actions;

    public Menu(string title, List<IMenuAction> options)
    {
        _title = title;
        _actions = options;
    }

    private void Show()
    {
        Console.Clear();
        Console.WriteLine(_title);
        foreach(var option in _actions)
        {
            if (_actions.IndexOf(option) == _cursor)
                Console.WriteLine($"> {option.Label}");
            else
                Console.WriteLine($"  {option.Label}");
        }
    }

    public async Task DisplayMenu()
    {
        bool keepRunning = true;

        while (keepRunning)
        {
            Show();
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (_cursor > 0)
                        _cursor--;
                    break;
                case ConsoleKey.DownArrow:
                    if (_cursor < _actions.Count - 1)
                        _cursor++;
                    break;
                case ConsoleKey.Enter:
                    keepRunning = await _actions[_cursor].ExecuteAsync();
                    break;
            }
        }
    }
}
