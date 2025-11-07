using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

public class Menu
{
    public string _title { get; private set; }

    public int _cursor { get; private set; } = 0;

    public int _lastCursor { get; private set; } = -1;

    public Stack<List<MenuAction>> _actionStack { get; private set; } = new();

    private Dictionary<ConsoleKey, MenuAction> _keyMap;

    private bool _running;
    private readonly IMenuRenderer _renderer;

    public Menu(
        string title,
        List<MenuAction> options,
        IMenuRenderer? renderer = null,
        Dictionary<ConsoleKey, MenuAction>? keyMap = null
        )
    {
        _title = title;
        _actionStack.Push(options);
        _renderer = renderer ?? new ConsoleMenuRenderer(new ConsoleMenuRendererOptions());
        _keyMap = keyMap ?? new()
        {
            {ConsoleKey.UpArrow, new Utility_MoveUp() },
            {ConsoleKey.DownArrow, new Menu_MoveDown() },
            {ConsoleKey.Enter, new Utility_Select() },
        };
    }

    

    private void Render()
    {
        var view = new MenuView(_title, _actionStack.Peek());
        if (_lastCursor == -1) _renderer.Initialize(view, _cursor);
        else if (_cursor != _lastCursor) _renderer.RenderSelectionChange(view, _lastCursor, _cursor);
        _lastCursor = _cursor;
    }

    public async Task DisplayMenu()
    {
        _running = true;

        while (_running)
        {
            Render();
            var key = Console.ReadKey(true).Key;

            if (_keyMap.TryGetValue(key, out var handler))
            {
                var running = await handler.ExecuteAsync(this);
                if (!running)
                    _running = false;
            }
        }
    }

    //setters

    public void SetTitle(string title) => _title = title;
    public void PushActions(List<MenuAction> actions)
    {
        _actionStack.Push(actions);
        RefreshAll();
    }
    public void AddActionMapping(ConsoleKey key, MenuAction action)
    {   
            _keyMap[key] = action;
    }
    public void RemoveActionMapping(ConsoleKey key)
    {
        if (_keyMap.ContainsKey(key))
            _keyMap.Remove(key);
    }
    public void MoveCursorUp()
    {
        if (_cursor > 0)
            _cursor--;
        else
            _cursor = _actionStack.Peek().Count - 1;
    }
    public void MoveCursorDown()
    {
        if (_cursor < _actionStack.Peek().Count - 1)
            _cursor++;
        else
            _cursor = 0;
    }

    public void RefreshAll()
    {
        var view = new MenuView(_title, _actionStack.Peek());
        _renderer.Rebuild(view, _cursor);
        _lastCursor = _cursor;
    }
    public void PopActions()
    {
        if (_actionStack.Count > 1)
            _actionStack.Pop();
        RefreshAll();
    }
}
