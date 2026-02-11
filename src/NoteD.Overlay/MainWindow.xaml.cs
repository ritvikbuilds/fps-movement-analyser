using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NoteD.Core;

namespace NoteD.Overlay;

public partial class MainWindow : Window
{
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    private RawInputListener? _inputListener;
    private readonly EventRingBuffer _eventBuffer = new(5000);
    private readonly DeltaCalculator _deltaCalculator;
    private readonly ConcurrentQueue<InputEvent> _pendingEvents = new();
    
    private readonly Stopwatch _fpsStopwatch = new();
    private int _frameCount;
    private double _currentFps;
    
    private readonly Dictionary<string, bool> _keyStates = new();
    private readonly Dictionary<string, double> _keyDownTimes = new();
    
    private const double TimelineSeconds = 5.0;
    
    private DeltaResult? _lastClickDelta;
    private double _lastClickTime;
    
    private bool _isClickThrough;
    private AppSettings _settings;
    private GlobalHotkey? _globalHotkey;
    private CsvEventLogger? _csvLogger;
    private bool _isLogging;
    private bool _isPaused;
    private double _pausedAtMs;
    private bool _renderingEnabled = true;

    public MainWindow()
    {
        InitializeComponent();
        
        _settings = AppSettings.Load();
        _deltaCalculator = new DeltaCalculator(_eventBuffer, lookupWindowMs: _settings.LookupWindowMs);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        StartInputCapture();
        RegisterHotkeys();
        _fpsStopwatch.Start();
        CompositionTarget.Rendering += OnRendering;
    }
    
    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_renderingEnabled) return;
        ProcessPendingEvents();
        UpdateFps();
        RenderTimeline();
    }

    private void RegisterHotkeys()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        _globalHotkey = new GlobalHotkey(hwnd);
        
        // Only Tab for pause - keeping it simple
        _globalHotkey.Register(_settings.Hotkeys.TogglePauseVisual, TogglePauseVisual);
        
        var source = HwndSource.FromHwnd(hwnd);
        source?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_globalHotkey?.ProcessMessage(msg, wParam) == true)
        {
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void ToggleLogging()
    {
        if (_isLogging)
        {
            _csvLogger?.Dispose();
            _csvLogger = null;
            _isLogging = false;
            StatusText.Text = " - Listening";
        }
        else
        {
            string logDir = string.IsNullOrEmpty(_settings.OutputDirectory) 
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : _settings.OutputDirectory;
            string logFile = System.IO.Path.Combine(logDir, $"noted_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
            _csvLogger = new CsvEventLogger(logFile);
            _isLogging = true;
            StatusText.Text = " - Recording";
        }
    }

    private void OpenSettingsWindow()
    {
        var settingsWindow = new SettingsWindow(_settings);
        settingsWindow.Owner = this;
        if (settingsWindow.ShowDialog() == true)
        {
            _deltaCalculator.LookupWindowMs = _settings.LookupWindowMs;
        }
    }

    private void ToggleOverlayVisibility()
    {
        Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
    }

    private void TogglePauseVisual()
    {
        _isPaused = !_isPaused;
        
        if (_isPaused)
        {
            _pausedAtMs = HighResolutionTimer.ToMilliseconds(HighResolutionTimer.GetTimestamp());
            StatusText.Text = " - [PAUSED]";
        }
        else
        {
            StatusText.Text = _isLogging ? " - Recording" : " - Listening";
        }
    }

    public void SetClickThrough(bool enable)
    {
        _isClickThrough = enable;
        var hwnd = new WindowInteropHelper(this).Handle;
        int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        
        if (enable)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        else
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
        }
    }

    public bool IsClickThrough => _isClickThrough;

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _renderingEnabled = false;
        CompositionTarget.Rendering -= OnRendering;
        _inputListener?.Dispose();
        _globalHotkey?.Dispose();
        _csvLogger?.Dispose();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void StartInputCapture()
    {
        try
        {
            _inputListener = new RawInputListener(OnInputEvent);
            _inputListener.Start();
            StatusText.Text = " - Listening";
        }
        catch (Exception ex)
        {
            StatusText.Text = $" - Error: {ex.Message}";
        }
    }

    private void OnInputEvent(InputEvent evt)
    {
        _pendingEvents.Enqueue(evt);
    }

    private void ProcessPendingEvents()
    {
        // When paused, discard all incoming events
        if (_isPaused)
        {
            while (_pendingEvents.TryDequeue(out _)) { }
            return;
        }
        
        while (_pendingEvents.TryDequeue(out var evt))
        {
            _eventBuffer.Add(evt);
            
            DeltaResult? deltas = null;
            
            if (evt.Device == InputDeviceType.Keyboard)
            {
                if (evt.Type == InputEventType.Down)
                {
                    _keyStates[evt.Key] = true;
                    _keyDownTimes[evt.Key] = evt.TimestampMs;
                }
                else
                {
                    _keyStates[evt.Key] = false;
                }
            }
            else if (evt.Device == InputDeviceType.Mouse && evt.Type == InputEventType.Down)
            {
                deltas = _deltaCalculator.ComputeDeltas(evt);
                _lastClickDelta = deltas;
                _lastClickTime = evt.TimestampMs;
                UpdateDeltaDisplay();
            }
            
            if (_isLogging && _csvLogger != null)
            {
                _csvLogger.Log(evt, deltas);
            }
        }
    }

    private void UpdateDeltaDisplay()
    {
        if (_lastClickDelta.HasValue)
        {
            var d = _lastClickDelta.Value;
            string text = "Last click: ";
            
            if (d.DeadzoneDeltaMs.HasValue)
            {
                text += $"Deadzone {d.DeadzoneDeltaMs.Value:0.0}ms";
            }
            if (d.CounterstrafeDeltaMs.HasValue)
            {
                if (d.DeadzoneDeltaMs.HasValue) text += " | ";
                text += $"Counter {d.CounterstrafeDeltaMs.Value:0.0}ms";
            }
            if (!d.DeadzoneDeltaMs.HasValue && !d.CounterstrafeDeltaMs.HasValue)
            {
                text += "-";
            }
            
            DeltaDisplay.Text = text;
        }
    }

    private void UpdateFps()
    {
        _frameCount++;
        if (_fpsStopwatch.ElapsedMilliseconds >= 1000)
        {
            _currentFps = _frameCount * 1000.0 / _fpsStopwatch.ElapsedMilliseconds;
            FpsDisplay.Text = $"{_currentFps:0} FPS";
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }
    }

    private void RenderTimeline()
    {
        var aParent = (FrameworkElement)ALaneImage.Parent;
        var dParent = (FrameworkElement)DLaneImage.Parent;
        var axisParent = (FrameworkElement)TimeAxisImage.Parent;
        
        double aWidth = aParent.ActualWidth;
        double aHeight = aParent.ActualHeight;
        double dWidth = dParent.ActualWidth;
        double dHeight = dParent.ActualHeight;
        double axisWidth = axisParent.ActualWidth;
        double axisHeight = axisParent.ActualHeight;
        
        if (aWidth <= 0 || aHeight <= 0) return;
        
        double nowMs = _isPaused 
            ? _pausedAtMs 
            : HighResolutionTimer.ToMilliseconds(HighResolutionTimer.GetTimestamp());
        double windowStartMs = nowMs - (TimelineSeconds * 1000);
        double windowDuration = nowMs - windowStartMs;
        
        var events = _eventBuffer.GetEventsInWindow(nowMs, TimelineSeconds * 1000);
        
        var aSegments = new List<(double start, double end)>();
        var dSegments = new List<(double start, double end)>();
        var keyDownTimes = new Dictionary<string, double>();
        var clicks = new List<(double time, string? activeKey, double? keyDownTime)>();
        
        double? aStart = _keyStates.GetValueOrDefault("A") ? windowStartMs : null;
        double? dStart = _keyStates.GetValueOrDefault("D") ? windowStartMs : null;
        
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evt = events[i];
            
            if (evt.Device == InputDeviceType.Keyboard)
            {
                if (evt.Key == "A")
                {
                    if (evt.Type == InputEventType.Down)
                    {
                        aStart = evt.TimestampMs;
                        keyDownTimes["A"] = evt.TimestampMs;
                    }
                    else if (aStart.HasValue)
                    {
                        aSegments.Add((aStart.Value, evt.TimestampMs));
                        aStart = null;
                    }
                }
                else if (evt.Key == "D")
                {
                    if (evt.Type == InputEventType.Down)
                    {
                        dStart = evt.TimestampMs;
                        keyDownTimes["D"] = evt.TimestampMs;
                    }
                    else if (dStart.HasValue)
                    {
                        dSegments.Add((dStart.Value, evt.TimestampMs));
                        dStart = null;
                    }
                }
            }
            else if (evt.Device == InputDeviceType.Mouse && evt.Type == InputEventType.Down)
            {
                string? activeKey = null;
                double? activeKeyDownTime = null;
                
                if (aStart.HasValue && keyDownTimes.TryGetValue("A", out double aDownTime))
                {
                    activeKey = "A";
                    activeKeyDownTime = aDownTime;
                }
                if (dStart.HasValue && keyDownTimes.TryGetValue("D", out double dDownTime))
                {
                    if (activeKey == null || dDownTime > activeKeyDownTime)
                    {
                        activeKey = "D";
                        activeKeyDownTime = dDownTime;
                    }
                }
                
                clicks.Add((evt.TimestampMs, activeKey, activeKeyDownTime));
            }
        }
        
        if (aStart.HasValue) aSegments.Add((aStart.Value, nowMs));
        if (dStart.HasValue) dSegments.Add((dStart.Value, nowMs));
        
        var aBrush = (SolidColorBrush)FindResource("AKeyBrush");
        var dBrush = (SolidColorBrush)FindResource("DKeyBrush");
        var clickBrush = (SolidColorBrush)FindResource("ClickBrush");
        var gridBrush = (SolidColorBrush)FindResource("GridLineBrush");
        var labelBg = new SolidColorBrush(Color.FromArgb(220, 30, 30, 45));
        var dotBorder = new Pen(new SolidColorBrush(Color.FromRgb(60, 60, 80)), 1);
        
        // Freeze brushes for performance
        labelBg.Freeze();
        dotBorder.Freeze();
        
        // Render A lane
        ALaneImage.Source = RenderLane(aWidth, aHeight, aSegments, aBrush, clicks, "A",
            clickBrush, labelBg, dotBorder, windowStartMs, windowDuration);
        
        // Render D lane
        DLaneImage.Source = RenderLane(dWidth, dHeight, dSegments, dBrush, clicks, "D",
            clickBrush, labelBg, dotBorder, windowStartMs, windowDuration);
        
        // Render time axis
        TimeAxisImage.Source = RenderTimeAxis(axisWidth, axisHeight, windowStartMs, nowMs, gridBrush);
    }

    private RenderTargetBitmap RenderLane(double width, double height,
        List<(double start, double end)> segments, Brush barBrush,
        List<(double time, string? activeKey, double? keyDownTime)> clicks, string laneKey,
        Brush clickBrush, Brush labelBg, Pen dotBorder,
        double windowStartMs, double windowDuration)
    {
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            // Draw bars - allow negative x for smooth slide-off
            foreach (var (start, end) in segments)
            {
                double x1 = ((start - windowStartMs) / windowDuration) * width;
                double x2 = ((end - windowStartMs) / windowDuration) * width;
                
                if (x2 <= 0 || x1 >= width) continue;
                
                dc.DrawRoundedRectangle(barBrush, null,
                    new Rect(x1, 2, x2 - x1, height - 4), 4, 4);
            }
            
            // Draw clicks on this lane
            foreach (var (clickTime, activeKey, keyDownTime) in clicks)
            {
                double x = ((clickTime - windowStartMs) / windowDuration) * width;
                if (x < 0 || x > width) continue;
                
                if (activeKey == laneKey)
                {
                    // White dot
                    dc.DrawEllipse(Brushes.White, dotBorder,
                        new Point(x, height / 2), 5, 5);
                    
                    // Timestamp label
                    if (keyDownTime.HasValue)
                    {
                        double deltaMs = clickTime - keyDownTime.Value;
                        var text = new FormattedText(
                            $"{deltaMs:0}ms",
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Medium, FontStretches.Normal),
                            11, Brushes.White, 1.0);
                        
                        double labelW = text.Width + 10;
                        double labelH = text.Height + 4;
                        double labelX = x - labelW / 2;
                        labelX = Math.Max(2, Math.Min(width - labelW - 2, labelX));
                        
                        dc.DrawRoundedRectangle(labelBg, null,
                            new Rect(labelX, 1, labelW, labelH), 3, 3);
                        dc.DrawText(text, new Point(labelX + 5, 3));
                    }
                }
                else if (activeKey == null)
                {
                    // Small faded dot
                    dc.PushOpacity(0.5);
                    dc.DrawEllipse(clickBrush, null,
                        new Point(x, height / 2), 4, 4);
                    dc.Pop();
                }
            }
        }
        
        var bmp = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(dv);
        return bmp;
    }

    private RenderTargetBitmap RenderTimeAxis(double width, double height,
        double windowStartMs, double nowMs, Brush brush)
    {
        var dv = new DrawingVisual();
        double windowDuration = nowMs - windowStartMs;
        
        using (var dc = dv.RenderOpen())
        {
            for (int i = 0; i <= (int)(windowDuration / 1000.0); i++)
            {
                double timeMs = nowMs - (i * 1000);
                double x = ((timeMs - windowStartMs) / windowDuration) * width;
                
                if (x < 0 || x > width) continue;
                
                var text = new FormattedText(
                    $"{i}s",
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    10, brush, 1.0);
                
                dc.PushOpacity(0.7);
                dc.DrawText(text, new Point(x - text.Width / 2, 2));
                dc.Pop();
            }
        }
        
        int h = Math.Max(1, (int)height);
        var bmp = new RenderTargetBitmap((int)width, h, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(dv);
        return bmp;
    }
}


