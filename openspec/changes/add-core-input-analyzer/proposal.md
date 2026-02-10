# Proposal: Add Core Input Analyzer

## Why

FPS players need precise feedback on their input timing to improve mechanics like counterstrafing (pressing opposite movement key before shooting) and deadzoning (releasing movement key before shooting). Existing tools either inject into games (anti-cheat risk) or lack sub-millisecond precision. NoteD will provide a safe, accurate, OBS-compatible overlay for training.

## What Changes

### New Capabilities
- **Input Capture System**: Raw Input API-based keyboard/mouse capture with QPC timestamps
- **Delta Computation Engine**: Calculate deadzone and counterstrafe timing deltas
- **Live Overlay Renderer**: Transparent, click-through WPF overlay with timeline visualization
- **Event Logging**: CSV/JSON export for offline analysis
- **Configuration System**: User-configurable keys, timing windows, visual settings
- **Offline Analyzer**: Python script for postgame timeline generation

### Technical Approach
- **Platform**: Windows 10/11 only
- **Stack**: C# / .NET 7 / WPF
- **Input API**: Raw Input (RegisterRawInputDevices, WM_INPUT) - external only, no injection
- **Timing**: QueryPerformanceCounter for sub-ms accuracy
- **Rendering**: WPF with hardware acceleration, 60-120 FPS

### Non-Goals
- Game injection or hooking (anti-cheat violation)
- Input synthesis or macro functionality
- Cross-platform support (Windows only for Raw Input)
- In-game integration beyond overlay capture

## Impact

### New Specs
- `input-capture/spec.md` - Raw Input capture and timestamping
- `delta-computation/spec.md` - Timing delta calculations
- `overlay-renderer/spec.md` - Live visualization
- `event-logging/spec.md` - CSV/JSON export
- `configuration/spec.md` - Settings and preferences
- `offline-analyzer/spec.md` - Postgame analysis tools

### New Code Modules
```
src/
├── NoteD.Core/                 # Core library (.NET 7 class library)
│   ├── Input/
│   │   ├── RawInputCapture.cs      # Raw Input API wrapper
│   │   ├── InputEvent.cs           # Event model
│   │   └── EventBuffer.cs          # Ring buffer
│   ├── Timing/
│   │   ├── HighResolutionTimer.cs  # QPC wrapper
│   │   └── DeltaCalculator.cs      # Deadzone/counterstrafe computation
│   └── Logging/
│       ├── CsvLogger.cs            # CSV export
│       └── JsonLogger.cs           # JSON export
├── NoteD.Overlay/              # WPF overlay app
│   ├── MainWindow.xaml             # Transparent overlay window
│   ├── TimelineRenderer.cs         # Timeline drawing logic
│   └── App.xaml                    # Application entry
├── NoteD.CLI/                  # Headless CLI for logging only
│   └── Program.cs
└── NoteD.Settings/             # Settings UI
    └── SettingsWindow.xaml

tools/
└── analyze.py                  # Offline analyzer script

tests/
├── NoteD.Core.Tests/
│   ├── TimingTests.cs
│   └── DeltaCalculatorTests.cs
```

### Safety Considerations
- Uses only external Raw Input APIs (no injection)
- Does not modify game memory or synthesize input
- Compatible with Vanguard, EAC, and other anti-cheats
- Documented API usage for transparency


