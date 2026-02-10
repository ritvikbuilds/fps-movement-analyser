using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
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
    
    private const double TimelineSeconds = 3.0;
    
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
        ALaneCanvas.Children.Clear();
        DLaneCanvas.Children.Clear();
        TimeAxisCanvas.Children.Clear();
        
        double aWidth = ALaneCanvas.ActualWidth;
        double aHeight = ALaneCanvas.ActualHeight;
        double dWidth = DLaneCanvas.ActualWidth;
        double dHeight = DLaneCanvas.ActualHeight;
        double axisWidth = TimeAxisCanvas.ActualWidth;
        
        if (aWidth <= 0 || aHeight <= 0) return;
        
        double nowMs = _isPaused 
            ? _pausedAtMs 
            : HighResolutionTimer.ToMilliseconds(HighResolutionTimer.GetTimestamp());
        double windowStartMs = nowMs - (TimelineSeconds * 1000);
        double windowDuration = nowMs - windowStartMs;
        
        var events = _eventBuffer.GetEventsInWindow(nowMs, TimelineSeconds * 1000);
        
        // Build segments and track key down times for delta calculation
        var aSegments = new List<(double start, double end)>();
        var dSegments = new List<(double start, double end)>();
        var keyDownTimes = new Dictionary<string, double>(); // track most recent key down time
        var clicks = new List<(double time, string? activeKey, double? keyDownTime)>();
        
        double? aStart = _keyStates.GetValueOrDefault("A") ? windowStartMs : null;
        double? dStart = _keyStates.GetValueOrDefault("D") ? windowStartMs : null;
        
        // Process oldest first
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
                // Find which key is active at click time and its down time
                string? activeKey = null;
                double? activeKeyDownTime = null;
                
                if (aStart.HasValue && keyDownTimes.TryGetValue("A", out double aDownTime))
                {
                    activeKey = "A";
                    activeKeyDownTime = aDownTime;
                }
                if (dStart.HasValue && keyDownTimes.TryGetValue("D", out double dDownTime))
                {
                    // If both keys held, use the most recently pressed one
                    if (activeKey == null || dDownTime > activeKeyDownTime)
                    {
                        activeKey = "D";
                        activeKeyDownTime = dDownTime;
                    }
                }
                
                clicks.Add((evt.TimestampMs, activeKey, activeKeyDownTime));
            }
        }
        
        // Close open segments
        if (aStart.HasValue) aSegments.Add((aStart.Value, nowMs));
        if (dStart.HasValue) dSegments.Add((dStart.Value, nowMs));
        
        var aBrush = (SolidColorBrush)FindResource("AKeyBrush");
        var dBrush = (SolidColorBrush)FindResource("DKeyBrush");
        var clickBrush = (SolidColorBrush)FindResource("ClickBrush");
        var gridBrush = (SolidColorBrush)FindResource("GridLineBrush");
        
        // Draw A lane bars and clicks
        foreach (var seg in aSegments)
        {
            DrawBar(ALaneCanvas, seg.start, seg.end, aHeight, aBrush, aWidth, windowStartMs, windowDuration);
        }
        
        // Draw D lane bars and clicks
        foreach (var seg in dSegments)
        {
            DrawBar(DLaneCanvas, seg.start, seg.end, dHeight, dBrush, dWidth, windowStartMs, windowDuration);
        }
        
        // Draw clicks with labels
        foreach (var (clickTime, activeKey, keyDownTime) in clicks)
        {
            double x = ((clickTime - windowStartMs) / windowDuration) * aWidth;
            if (x < 0 || x > aWidth) continue;
            
            // Draw dot on appropriate lane
            if (activeKey == "A")
            {
                DrawClickOnLane(ALaneCanvas, x, aHeight, clickBrush, clickTime, keyDownTime);
            }
            else if (activeKey == "D")
            {
                DrawClickOnLane(DLaneCanvas, x, dHeight, clickBrush, clickTime, keyDownTime);
            }
            else
            {
                // No active key - draw small dot on both lanes without label
                DrawSmallDot(ALaneCanvas, x, aHeight, clickBrush);
                DrawSmallDot(DLaneCanvas, x, dHeight, clickBrush);
            }
        }
        
        // Draw time axis
        DrawTimeAxis(axisWidth, windowStartMs, nowMs, gridBrush);
    }

    private void DrawBar(Canvas canvas, double startMs, double endMs, double height, Brush brush,
                         double canvasWidth, double windowStartMs, double windowDuration)
    {
        double x1 = ((startMs - windowStartMs) / windowDuration) * canvasWidth;
        double x2 = ((endMs - windowStartMs) / windowDuration) * canvasWidth;
        
        x1 = Math.Max(0, x1);
        x2 = Math.Min(canvasWidth, x2);
        
        if (x2 <= x1) return;
        
        var rect = new Rectangle
        {
            Width = x2 - x1,
            Height = height - 4,
            Fill = brush,
            RadiusX = 4,
            RadiusY = 4
        };
        
        Canvas.SetLeft(rect, x1);
        Canvas.SetTop(rect, 2);
        canvas.Children.Add(rect);
    }

    private void DrawClickOnLane(Canvas canvas, double x, double height, Brush brush, double clickTime, double? keyDownTime)
    {
        // Draw dot - white fill with subtle dark border
        var dot = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.White,
            Stroke = new SolidColorBrush(Color.FromRgb(60, 60, 80)),
            StrokeThickness = 1
        };
        Canvas.SetLeft(dot, x - 5);
        Canvas.SetTop(dot, height / 2 - 5);
        canvas.Children.Add(dot);
        
        // Draw timestamp label (time from key down)
        if (keyDownTime.HasValue)
        {
            double deltaMs = clickTime - keyDownTime.Value;
            string labelText = $"{deltaMs:0}ms";
            
            var label = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 45)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(5, 2, 5, 2),
                Child = new TextBlock
                {
                    Text = labelText,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontWeight = FontWeights.Medium
                }
            };
            
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double labelX = x - label.DesiredSize.Width / 2;
            labelX = Math.Max(2, Math.Min(canvas.ActualWidth - label.DesiredSize.Width - 2, labelX));
            
            Canvas.SetLeft(label, labelX);
            Canvas.SetTop(label, 2);
            canvas.Children.Add(label);
        }
    }

    private void DrawSmallDot(Canvas canvas, double x, double height, Brush brush)
    {
        var dot = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = brush,
            Opacity = 0.5
        };
        Canvas.SetLeft(dot, x - 4);
        Canvas.SetTop(dot, height / 2 - 4);
        canvas.Children.Add(dot);
    }

    private void DrawTimeAxis(double width, double windowStartMs, double nowMs, Brush brush)
    {
        double windowDuration = nowMs - windowStartMs;
        double secondsInWindow = windowDuration / 1000.0;
        
        for (int i = 0; i <= (int)secondsInWindow; i++)
        {
            double timeMs = nowMs - (i * 1000);
            double x = ((timeMs - windowStartMs) / windowDuration) * width;
            
            if (x < 0 || x > width) continue;
            
            var label = new TextBlock
            {
                Text = $"{i}s",
                Foreground = brush,
                FontSize = 10,
                Opacity = 0.7
            };
            
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, 2);
            TimeAxisCanvas.Children.Add(label);
        }
    }
}


