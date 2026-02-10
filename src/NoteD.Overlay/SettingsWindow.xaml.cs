using System.Windows;
using NoteD.Core;

namespace NoteD.Overlay;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
        CheckA.IsChecked = _settings.MonitoredKeys.Contains("A");
        CheckD.IsChecked = _settings.MonitoredKeys.Contains("D");
        CheckW.IsChecked = _settings.MonitoredKeys.Contains("W");
        CheckS.IsChecked = _settings.MonitoredKeys.Contains("S");
        
        LookupWindowInput.Text = _settings.LookupWindowMs.ToString();
        DebounceInput.Text = _settings.DebounceMs.ToString();
        
        OpacitySlider.Value = _settings.Overlay.Opacity;
        TimelineSecondsInput.Text = _settings.Overlay.TimelineSeconds.ToString();
        FontSizeInput.Text = _settings.Overlay.FontSize.ToString();
        
        HotkeyLoggingInput.Text = _settings.Hotkeys.ToggleLogging;
        HotkeySettingsInput.Text = _settings.Hotkeys.OpenSettings;
        HotkeyOverlayInput.Text = _settings.Hotkeys.ToggleOverlay;
        
        OutputPathInput.Text = _settings.OutputDirectory;
    }

    private void SaveSettings()
    {
        _settings.MonitoredKeys.Clear();
        if (CheckA.IsChecked == true) _settings.MonitoredKeys.Add("A");
        if (CheckD.IsChecked == true) _settings.MonitoredKeys.Add("D");
        if (CheckW.IsChecked == true) _settings.MonitoredKeys.Add("W");
        if (CheckS.IsChecked == true) _settings.MonitoredKeys.Add("S");
        
        if (double.TryParse(LookupWindowInput.Text, out double lookupWindow))
            _settings.LookupWindowMs = lookupWindow;
        if (double.TryParse(DebounceInput.Text, out double debounce))
            _settings.DebounceMs = debounce;
        
        _settings.Overlay.Opacity = OpacitySlider.Value;
        if (double.TryParse(TimelineSecondsInput.Text, out double timelineSec))
            _settings.Overlay.TimelineSeconds = timelineSec;
        if (int.TryParse(FontSizeInput.Text, out int fontSize))
            _settings.Overlay.FontSize = fontSize;
        
        _settings.Hotkeys.ToggleLogging = HotkeyLoggingInput.Text;
        _settings.Hotkeys.OpenSettings = HotkeySettingsInput.Text;
        _settings.Hotkeys.ToggleOverlay = HotkeyOverlayInput.Text;
        
        _settings.OutputDirectory = OutputPathInput.Text;
        
        _settings.Save();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
