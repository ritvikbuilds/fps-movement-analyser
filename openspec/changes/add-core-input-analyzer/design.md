# Design Document: NoteD Input Analyzer

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        NoteD Application                         │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐    ┌──────────────┐    ┌─────────────────┐    │
│  │  Raw Input  │───▶│ Event Buffer │───▶│ Delta Calculator│    │
│  │   Capture   │    │ (Ring Buffer)│    │                 │    │
│  └─────────────┘    └──────────────┘    └────────┬────────┘    │
│         │                   │                     │             │
│         │                   ▼                     ▼             │
│         │           ┌──────────────┐    ┌─────────────────┐    │
│         │           │  CSV Logger  │    │ Overlay Renderer│    │
│         │           └──────────────┘    └─────────────────┘    │
│         │                                                       │
│         ▼                                                       │
│  ┌─────────────┐                                               │
│  │   Windows   │  (WM_INPUT messages via Raw Input API)        │
│  │   Message   │                                               │
│  │    Loop     │                                               │
│  └─────────────┘                                               │
└─────────────────────────────────────────────────────────────────┘
```

## Thread Model

```
Thread 1: UI Thread (WPF Dispatcher)
├── Handles WM_INPUT messages
├── Updates overlay at 60-120 FPS
└── Processes settings UI

Thread 2: Background Logger (optional)
├── Consumes events from queue
└── Writes to CSV file asynchronously
```

## Key Design Decisions

### 1. Raw Input API (Not Hooks)

**Decision**: Use `RegisterRawInputDevices` with `RIDEV_INPUTSINK` flag.

**Rationale**:
- Does NOT inject into any process
- Does NOT use SetWindowsHookEx (which can trigger anti-cheat)
- Receives input messages even when not focused
- Officially supported by Microsoft
- Same API used by many legitimate apps (input recorders, accessibility tools)

**Anti-Cheat Safety**:
- Vanguard/FACEIT/EAC do not flag Raw Input consumers
- We never call: `SendInput`, `keybd_event`, `mouse_event`
- We never use: `SetWindowsHookEx`, `WriteProcessMemory`, `CreateRemoteThread`

### 2. QueryPerformanceCounter for Timing

**Decision**: Use QPC instead of `DateTime.Now` or `Environment.TickCount`.

**Rationale**:
- Sub-microsecond resolution
- Monotonic (never jumps backward)
- Not affected by system time changes
- Standard for game/audio timing

**Implementation**:
```csharp
[DllImport("kernel32.dll")]
static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

[DllImport("kernel32.dll")]
static extern bool QueryPerformanceFrequency(out long lpFrequency);
```

### 3. WPF for Overlay (Not WinForms/DirectX)

**Decision**: Use WPF with `AllowsTransparency=True` and hardware acceleration.

**Rationale**:
- Per-pixel transparency works out-of-box
- GPU-accelerated rendering via DirectX
- Easy to style and animate
- Lower development complexity than raw DirectX

**Click-Through Implementation**:
```csharp
// Extended window style for click-through
const int WS_EX_TRANSPARENT = 0x00000020;
const int GWL_EXSTYLE = -20;

// Apply during window load
SetWindowLong(hwnd, GWL_EXSTYLE, 
    GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_TRANSPARENT);
```

### 4. Ring Buffer for Events

**Decision**: Fixed-size ring buffer (default: 30 seconds of events).

**Rationale**:
- Bounded memory usage
- O(1) insertion
- No allocations during steady state
- Easy backward search for delta computation

**Sizing**: At 1000 events/second max (extreme case), 30 seconds = 30,000 events × ~64 bytes = ~2 MB max.

### 5. Event Queue for Thread Decoupling

**Decision**: Use `ConcurrentQueue<InputEvent>` between capture and rendering.

**Rationale**:
- Input capture happens in message pump (UI thread)
- Rendering happens on CompositionTarget.Rendering (UI thread)
- Logging can happen on background thread
- Lock-free queue prevents jank

## Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| CPU Usage | < 5% | Task Manager during active use |
| Memory | < 50 MB | Working set |
| Render FPS | 60-120 | Overlay smoothness |
| Input Latency | < 1 ms | Time from hardware event to buffer |
| Event Drop Rate | 0% | Under burst input |

## File Format: CSV

```csv
timestamp_qpc,timestamp_ms,device,key,event_type,deadzone_delta_ms,counter_delta_ms
1234567890123,1000.0,keyboard,A,down,,
1234567890456,1000.3,keyboard,A,up,,
1234567890789,1000.6,mouse,Mouse1,click,0.3,
```

## Configuration File: JSON

```json
{
  "monitoredKeys": ["A", "D"],
  "lookupWindowMs": 200,
  "debounceMs": 2,
  "bufferRetentionSeconds": 30,
  "overlay": {
    "opacity": 1.0,
    "timelineSeconds": 5,
    "colors": {
      "A": "#FFA500",
      "D": "#0066FF",
      "click": "#FF0000"
    },
    "fontSize": 12
  },
  "hotkeys": {
    "toggleLogging": "F9",
    "openSettings": "F8"
  },
  "outputDirectory": "%USERPROFILE%/Documents/NoteD"
}
```

## Security Considerations

### What We DO:
- Register for Raw Input (read-only, external)
- Read system performance counter
- Create transparent overlay window
- Write files to user's documents folder

### What We DON'T DO:
- Inject code into any process
- Hook keyboard/mouse at system level
- Modify game memory
- Synthesize or block input
- Run with elevated privileges (admin not required)
- Access game process in any way

### API Whitelist (for anti-cheat documentation):
```
kernel32.dll: QueryPerformanceCounter, QueryPerformanceFrequency
user32.dll: RegisterRawInputDevices, GetRawInputData, SetWindowLong, GetWindowLong
(Standard WPF/CLR APIs for window management)
```


