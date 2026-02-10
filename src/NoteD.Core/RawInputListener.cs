using System.ComponentModel;
using System.Runtime.InteropServices;

namespace NoteD.Core;

public enum InputDeviceType
{
    Keyboard,
    Mouse
}

public enum InputEventType
{
    Down,
    Up
}

public record struct InputEvent(
    long TimestampQpc,
    double TimestampMs,
    InputDeviceType Device,
    string Key,
    InputEventType Type
);

public class RawInputListener : IDisposable
{
    private Thread? _messageLoopThread;
    private volatile bool _isRunning;
    private IntPtr _hwnd;
    private readonly Action<InputEvent> _onEvent;
    private NativeMethods.WndProc? _wndProcDelegate; // prevent GC

    // Filter keys
    private readonly HashSet<ushort> _monitoredVKeys = new()
    {
        0x41, // A
        0x44, // D
        0x57, // W
        0x53, // S
        0x25, // Left Arrow
        0x26, // Up Arrow
        0x27, // Right Arrow
        0x28  // Down Arrow
    };

    public RawInputListener(Action<InputEvent> onEvent)
    {
        _onEvent = onEvent;
    }

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        _messageLoopThread = new Thread(MessageLoop)
        {
            IsBackground = true,
            Name = "RawInputMessageLoop"
        };
        _messageLoopThread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        if (_hwnd != IntPtr.Zero)
        {
            NativeMethods.DestroyWindow(_hwnd);
        }
    }

    private void MessageLoop()
    {
        // 1. Create Message-Only Window
        string className = $"NoteDRawInputClass_{Environment.TickCount}";
        
        // Keep delegate alive to prevent GC
        _wndProcDelegate = WndProc;
        
        var wndClass = new NativeMethods.WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<NativeMethods.WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = NativeMethods.GetModuleHandle(null),
            lpszClassName = className
        };

        ushort classAtom = NativeMethods.RegisterClassEx(ref wndClass);
        if (classAtom == 0)
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, $"Failed to register window class. Error: {error}");
        }

        _hwnd = NativeMethods.CreateWindowEx(
            0, className, "NoteDRawInputWindow", 0, 0, 0, 0, 0,
            (IntPtr)(-3), // HWND_MESSAGE
            IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, $"Failed to create message-only window. Error: {error}");
        }

        // 2. Register Raw Input
        var devices = new NativeMethods.RAWINPUTDEVICE[2];
        
        // Keyboard
        devices[0].usUsagePage = 0x01; // Generic Desktop
        devices[0].usUsage = 0x06;     // Keyboard
        devices[0].dwFlags = NativeMethods.RIDEV_INPUTSINK;
        devices[0].hwndTarget = _hwnd;

        // Mouse
        devices[1].usUsagePage = 0x01; // Generic Desktop
        devices[1].usUsage = 0x02;     // Mouse
        devices[1].dwFlags = NativeMethods.RIDEV_INPUTSINK;
        devices[1].hwndTarget = _hwnd;

        if (!NativeMethods.RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<NativeMethods.RAWINPUTDEVICE>()))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register raw input devices");
        }

        // 3. Message Loop
        NativeMethods.MSG msg;
        while (_isRunning && NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
        }
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == NativeMethods.WM_INPUT)
        {
            ProcessRawInput(lParam);
        }
        return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private unsafe void ProcessRawInput(IntPtr hRawInput)
    {
        uint pcbSize = 0;
        NativeMethods.GetRawInputData(hRawInput, NativeMethods.RID_INPUT, IntPtr.Zero, ref pcbSize, (uint)Marshal.SizeOf<NativeMethods.RAWINPUTHEADER>());

        if (pcbSize == 0) return;

        IntPtr pData = Marshal.AllocHGlobal((int)pcbSize);
        try
        {
            if (NativeMethods.GetRawInputData(hRawInput, NativeMethods.RID_INPUT, pData, ref pcbSize, (uint)Marshal.SizeOf<NativeMethods.RAWINPUTHEADER>()) != pcbSize)
            {
                return;
            }

            var rawInput = (NativeMethods.RAWINPUT*)pData;
            long timestampQpc = HighResolutionTimer.GetTimestamp();
            double timestampMs = HighResolutionTimer.ToMilliseconds(timestampQpc);

            if (rawInput->header.dwType == NativeMethods.RIM_TYPEKEYBOARD)
            {
                ProcessKeyboard(rawInput->keyboard, timestampQpc, timestampMs);
            }
            else if (rawInput->header.dwType == NativeMethods.RIM_TYPEMOUSE)
            {
                ProcessMouse(rawInput->mouse, timestampQpc, timestampMs);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(pData);
        }
    }

    private void ProcessKeyboard(NativeMethods.RAWKEYBOARD kb, long qpc, double ms)
    {
        // Filter out key repeats if desired (not strictly necessary for raw input as flags handles it, but good to check)
        // RI_KEY_BREAK = 1 (Up), RI_KEY_MAKE = 0 (Down)
        bool isUp = (kb.Flags & 1) == 1;
        bool isE0 = (kb.Flags & 2) == 2;
        
        ushort vkey = kb.VKey;
        if (vkey == 0) return; // Sometimes VKey is 0, use MakeCode if needed (omitted for MVP simplicity)

        if (!_monitoredVKeys.Contains(vkey)) return;

        string keyName = ((Keys)vkey).ToString(); // Simple casting for MVP
        
        _onEvent(new InputEvent(
            qpc,
            ms,
            InputDeviceType.Keyboard,
            keyName,
            isUp ? InputEventType.Up : InputEventType.Down
        ));
    }

    private void ProcessMouse(NativeMethods.RAWMOUSE mouse, long qpc, double ms)
    {
        // Check for Left Button
        // RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001
        // RI_MOUSE_LEFT_BUTTON_UP = 0x0002
        
        if ((mouse.ulButtons & 0x0001) != 0) // Down
        {
            _onEvent(new InputEvent(qpc, ms, InputDeviceType.Mouse, "MouseLeft", InputEventType.Down));
        }
        
        if ((mouse.ulButtons & 0x0002) != 0) // Up
        {
            _onEvent(new InputEvent(qpc, ms, InputDeviceType.Mouse, "MouseLeft", InputEventType.Up));
        }
    }

    public void Dispose()
    {
        Stop();
    }
}

// Minimal Keys enum for display
public enum Keys : ushort
{
    Left = 0x25,
    Up = 0x26,
    Right = 0x27,
    Down = 0x28,
    A = 0x41,
    D = 0x44,
    S = 0x53,
    W = 0x57
}


