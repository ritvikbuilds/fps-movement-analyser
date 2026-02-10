using System.Runtime.InteropServices;

namespace NoteD.Core;

public class GlobalHotkey : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_NONE = 0x0000;
    
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly IntPtr _hwnd;
    private readonly Dictionary<int, Action> _hotkeys = new();
    private int _nextId = 1;
    private bool _disposed;

    public GlobalHotkey(IntPtr windowHandle)
    {
        _hwnd = windowHandle;
    }

    public int Register(string key, Action callback)
    {
        uint vk = KeyToVirtualKey(key);
        if (vk == 0) return -1;
        
        int id = _nextId++;
        if (RegisterHotKey(_hwnd, id, MOD_NONE, vk))
        {
            _hotkeys[id] = callback;
            return id;
        }
        return -1;
    }

    public void Unregister(int id)
    {
        if (_hotkeys.ContainsKey(id))
        {
            UnregisterHotKey(_hwnd, id);
            _hotkeys.Remove(id);
        }
    }

    public bool ProcessMessage(int msg, IntPtr wParam)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_hotkeys.TryGetValue(id, out var callback))
            {
                callback();
                return true;
            }
        }
        return false;
    }

    private static uint KeyToVirtualKey(string key)
    {
        return key.ToUpperInvariant() switch
        {
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            "TAB" => 0x09,
            "CAPSLOCK" => 0x14,
            "INSERT" => 0x2D,
            "PAUSE" => 0x13,
            "SCROLLLOCK" => 0x91,
            _ => 0
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        foreach (var id in _hotkeys.Keys.ToList())
        {
            UnregisterHotKey(_hwnd, id);
        }
        _hotkeys.Clear();
    }
}
