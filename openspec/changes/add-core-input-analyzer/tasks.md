# Implementation Tasks

## Phase 1: Core Infrastructure (MVP - CLI Logger) ✅ COMPLETE

### 1.1 Project Setup ✅
- [x] Create .NET 7 solution with projects: NoteD.Core, NoteD.CLI, NoteD.Core.Tests
- [x] Set up project references and NuGet packages
- [x] Create solution directory structure

### 1.2 High-Resolution Timing ✅
- [x] Implement `HighResolutionTimer` class with P/Invoke for QueryPerformanceCounter/Frequency
- [x] Add `ToMilliseconds()` and `GetTimestamp()` methods
- [x] Write unit tests for timing accuracy

### 1.3 Event Model & Buffer ✅
- [x] Define `InputEvent` record with all required fields
- [x] Implement thread-safe `EventRingBuffer` with configurable capacity
- [x] Write unit tests for buffer overflow and retrieval

### 1.4 Raw Input Capture ✅
- [x] Implement P/Invoke declarations for Raw Input API (RegisterRawInputDevices, GetRawInputData)
- [x] Create `RawInputListener` class to handle WM_INPUT messages
- [x] Parse keyboard events (keydown/keyup) with VKey mapping
- [x] Parse mouse events (left button down/up)

### 1.5 CSV Logger ✅
- [x] Implement `CsvEventLogger` class with async file writing
- [x] Add proper header row with delta columns
- [x] Handle file path configuration and session naming

### 1.6 CLI Application ✅
- [x] Create NoteD.CLI console application
- [x] Implement headless capture mode with CSV output
- [x] Integrate delta computation in real-time

## Phase 2: Delta Computation

### 2.1 Delta Calculator
- [x] Implement `DeltaCalculator` class
- [x] Add `ComputeDeadzoneDealta()` - find last keyup before click
- [x] Add `ComputeCounterstrafeData()` - find opposite keydown before click
- [x] Implement configurable lookup window
- [x] Write comprehensive unit tests with edge cases

### 2.2 Integrate with Logger
- [x] Update CSV logger to include delta columns
- [x] Compute deltas in real-time as events arrive
- [x] Test delta output in CLI mode

## Phase 3: Overlay Renderer

### 3.1 WPF Overlay Window
- [x] Create WPF project with transparent window (AllowsTransparency, WindowStyle=None)
- [x] Configure always-on-top (Topmost=true)
- [x] Implement click-through using WS_EX_TRANSPARENT extended style
- [x] Set up OBS-compatible window capture

### 3.2 Timeline Renderer
- [x] Implement `TimelineRenderer` class with WPF drawing
- [x] Draw key lane backgrounds and labels
- [x] Draw colored bars for key holds (orange A, blue D)
- [x] Draw red dots for mouse clicks
- [x] Implement scrolling timeline with time axis

### 3.3 Delta Labels
- [x] Render ms labels above click dots
- [x] Style with white text on dark rounded background
- [x] Format to one decimal place

### 3.4 Performance Optimization
- [x] Decouple input capture thread from UI thread
- [x] Use ConcurrentQueue for event passing
- [x] Implement render list to avoid locking
- [x] Target 60+ FPS rendering

## Phase 4: Configuration & Settings

### 4.1 Settings Model
- [x] Define `AppSettings` class with all configuration properties
- [x] Implement JSON serialization for settings persistence
- [x] Add settings file location (AppData/NoteD/)

### 4.2 Settings Window
- [x] Create WPF settings window
- [x] Add key selection UI (checkboxes or list)
- [x] Add timing configuration (lookup window, debounce)
- [x] Add visual configuration (colors, opacity, font)
- [x] Add hotkey configuration
- [x] Add output path configuration

### 4.3 Hotkey System
- [x] Implement global hotkey registration
- [x] Add logging toggle hotkey (F9)
- [x] Add settings window hotkey (F8)
- [x] Add overlay toggle hotkey
- [x] Add pause visual hotkey (Tab) - freezes timeline while capture continues

## Phase 5: Polish & Documentation

### 5.1 Offline Analyzer
- [x] Create Python script `analyze.py`
- [x] Load CSV and compute statistics
- [x] Generate PNG timeline visualization
- [ ] Generate HTML/D3 interactive timeline (optional)

### 5.2 Documentation
- [ ] Write README with build instructions
- [ ] Document OBS capture setup
- [ ] Document anti-cheat safety (APIs used, why safe)
- [ ] Include example CSV output
(Documentation tasks deferred - user did not request documentation)

### 5.3 Testing & Validation
- [ ] Complete unit test suite
- [ ] Create integration test script
- [ ] Write human test plan
- [ ] Record demo video/GIF
- [ ] Generate performance report (CPU/memory)

### 5.4 Release
- [ ] Create release build configuration
- [ ] Build self-contained executable
- [ ] Create simple installer or portable ZIP
- [x] Add MIT license file


