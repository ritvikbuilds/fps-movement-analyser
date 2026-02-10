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
    
    private readonly DispatcherTimer _renderTimer;
    private readonly Stopwatch _fpsStopwatch = new();
    private int _frameCount;
    private double _currentFps;
    
    private readonly Dictionary<string, bool> _keyStates = new();
    private readonly Dictionary<string, double> _keyDownTimes = new();
    
    private const double TimelineSeconds = 3.0;
    private const double PixelsPerSecond = 140.0;
    
    private DeltaResult? _lastClickDelta;
    private double _lastClickTime;
    
    private bool _isClickThrough;
    private AppSettings _settings;
    private GlobalHotkey? _globalHotkey;
    private CsvEventLogger? _csvLogger;
    private bool _isLogging;
    private bool _isPaused;
    private double _pausedAtMs;

    public MainWindow()
    {
        InitializeComponent();
        
        _settings = AppSettings.Load();
        _deltaCalculator = new DeltaCalculator(_eventBuffer, lookupWindowMs: _settings.LookupWindowMs);
        
        _renderTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16.67) // ~60 FPS
        };
        _renderTimer.Tick += RenderTimer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        StartInputCapture();
        RegisterHotkeys();
        _fpsStopwatch.Start();
        _renderTimer.Start();
    }

    private void RegisterHotkeys()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        _globalHotkey = new GlobalHotkey(hwnd);
        
        _globalHotkey.Register(_settings.Hotkeys.ToggleLogging, ToggleLogging);
        _globalHotkey.Register(_settings.Hotkeys.OpenSettings, OpenSettingsWindow);
        _globalHotkey.Register(_settings.Hotkeys.ToggleOverlay, ToggleOverlayVisibility);
        _globalHotkey.Register(_settings.Hotkeys.ToggleClickThrough, () => SetClickThrough(!_isClickThrough));
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
        _renderTimer.Stop();
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

    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        ProcessPendingEvents();
        UpdateFps();
        RenderTimeline();
    }

    private void ProcessPendingEvents()
    {
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
        TimelineCanvas.Children.Clear();
        ClickCanvas.Children.Clear();
        
        double canvasWidth = TimelineCanvas.ActualWidth;
        double canvasHeight = TimelineCanvas.ActualHeight;
        
        if (canvasWidth <= 0 || canvasHeight <= 0) return;
        
        double laneHeight = canvasHeight / 2.0;
        double nowMs = _isPaused 
            ? _pausedAtMs 
            : HighResolutionTimer.ToMilliseconds(HighResolutionTimer.GetTimestamp());
        double windowStartMs = nowMs - (TimelineSeconds * 1000);
        
        // Draw grid lines
        DrawGridLines(canvasWidth, canvasHeight, nowMs, windowStartMs);
        
        // Get recent events
        var events = _eventBuffer.GetEventsInWindow(nowMs, TimelineSeconds * 1000);
        
        // Track bar segments for A and D keys
        var aSegments = new List<(double start, double end)>();
        var dSegments = new List<(double start, double end)>();
        var clicks = new List<(double time, DeltaResult? delta)>();
        
        // Process events to build segments
        double? aStart = _keyStates.GetValueOrDefault("A") ? windowStartMs : null;
        double? dStart = _keyStates.GetValueOrDefault("D") ? windowStartMs : null;
        
        // Process in reverse (oldest first)
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evt = events[i];
            
            if (evt.Device == InputDeviceType.Keyboard)
            {
                if (evt.Key == "A")
                {
                    if (evt.Type == InputEventType.Down)
                        aStart = evt.TimestampMs;
                    else if (aStart.HasValue)
                    {
                        aSegments.Add((aStart.Value, evt.TimestampMs));
                        aStart = null;
                    }
                }
                else if (evt.Key == "D")
                {
                    if (evt.Type == InputEventType.Down)
                        dStart = evt.TimestampMs;
                    else if (dStart.HasValue)
                    {
                        dSegments.Add((dStart.Value, evt.TimestampMs));
                        dStart = null;
                    }
                }
            }
            else if (evt.Device == InputDeviceType.Mouse && evt.Type == InputEventType.Down)
            {
                var delta = _deltaCalculator.ComputeDeltas(evt);
                clicks.Add((evt.TimestampMs, delta));
            }
        }
        
        // Close open segments at current time
        if (aStart.HasValue) aSegments.Add((aStart.Value, nowMs));
        if (dStart.HasValue) dSegments.Add((dStart.Value, nowMs));
        
        // Draw key bars
        var aBrush = (SolidColorBrush)FindResource("AKeyBrush");
        var dBrush = (SolidColorBrush)FindResource("DKeyBrush");
        var clickBrush = (SolidColorBrush)FindResource("ClickBrush");
        
        foreach (var seg in aSegments)
        {
            DrawKeyBar(seg.start, seg.end, 0, laneHeight, aBrush, canvasWidth, windowStartMs, nowMs);
        }
        
        foreach (var seg in dSegments)
        {
            DrawKeyBar(seg.start, seg.end, laneHeight, laneHeight, dBrush, canvasWidth, windowStartMs, nowMs);
        }
        
        // Draw click markers
        foreach (var click in clicks)
        {
            DrawClickMarker(click.time, click.delta, canvasWidth, canvasHeight, windowStartMs, nowMs, clickBrush);
        }
    }

    private void DrawGridLines(double width, double height, double nowMs, double windowStartMs)
    {
        var gridBrush = (SolidColorBrush)FindResource("GridLineBrush");
        double msPerLine = 500; // Grid line every 500ms
        
        double startLineMs = Math.Ceiling(windowStartMs / msPerLine) * msPerLine;
        
        for (double lineMs = startLineMs; lineMs < nowMs; lineMs += msPerLine)
        {
            double x = ((lineMs - windowStartMs) / (nowMs - windowStartMs)) * width;
            
            var line = new Line
            {
                X1 = x, Y1 = 0,
                X2 = x, Y2 = height,
                Stroke = gridBrush,
                StrokeThickness = 1,
                Opacity = 0.3
            };
            TimelineCanvas.Children.Add(line);
        }
        
        // Draw center line between lanes
        var centerLine = new Line
        {
            X1 = 0, Y1 = height / 2,
            X2 = width, Y2 = height / 2,
            Stroke = gridBrush,
            StrokeThickness = 1,
            Opacity = 0.5
        };
        TimelineCanvas.Children.Add(centerLine);
    }

    private void DrawKeyBar(double startMs, double endMs, double y, double height, Brush brush, 
                            double canvasWidth, double windowStartMs, double nowMs)
    {
        double windowDuration = nowMs - windowStartMs;
        
        double x1 = ((startMs - windowStartMs) / windowDuration) * canvasWidth;
        double x2 = ((endMs - windowStartMs) / windowDuration) * canvasWidth;
        
        x1 = Math.Max(0, x1);
        x2 = Math.Min(canvasWidth, x2);
        
        if (x2 <= x1) return;
        
        var rect = new Rectangle
        {
            Width = x2 - x1,
            Height = height - 8,
            Fill = brush,
            RadiusX = 3,
            RadiusY = 3,
            Opacity = 0.85
        };
        
        Canvas.SetLeft(rect, x1);
        Canvas.SetTop(rect, y + 4);
        TimelineCanvas.Children.Add(rect);
    }

    private void DrawClickMarker(double clickMs, DeltaResult? delta, double canvasWidth, double canvasHeight,
                                  double windowStartMs, double nowMs, Brush brush)
    {
        double windowDuration = nowMs - windowStartMs;
        double x = ((clickMs - windowStartMs) / windowDuration) * canvasWidth;
        
        if (x < 0 || x > canvasWidth) return;
        
        // Draw vertical line through both lanes
        var line = new Line
        {
            X1 = x, Y1 = 0,
            X2 = x, Y2 = canvasHeight,
            Stroke = brush,
            StrokeThickness = 2,
            Opacity = 0.9
        };
        TimelineCanvas.Children.Add(line);
        
        // Draw click dot
        var dot = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = brush
        };
        Canvas.SetLeft(dot, x - 5);
        Canvas.SetTop(dot, canvasHeight / 2 - 5);
        TimelineCanvas.Children.Add(dot);
        
        // Draw delta label in click canvas
        if (delta.HasValue && (delta.Value.DeadzoneDeltaMs.HasValue || delta.Value.CounterstrafeDeltaMs.HasValue))
        {
            string labelText;
            if (delta.Value.DeadzoneDeltaMs.HasValue)
                labelText = $"{delta.Value.DeadzoneDeltaMs.Value:0.0}";
            else
                labelText = $"{delta.Value.CounterstrafeDeltaMs!.Value:0.0}";
            
            var label = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 50)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 1, 4, 1),
                Child = new TextBlock
                {
                    Text = labelText,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontFamily = new FontFamily("Consolas"),
                    FontWeight = FontWeights.Bold
                }
            };
            
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double labelX = x - label.DesiredSize.Width / 2;
            labelX = Math.Max(0, Math.Min(canvasWidth - label.DesiredSize.Width, labelX));
            
            Canvas.SetLeft(label, labelX);
            Canvas.SetTop(label, 2);
            ClickCanvas.Children.Add(label);
        }
    }
}


