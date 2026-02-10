# Proposal: Core Input Capture & Logging

## Why
To enable FPS players to analyze their mechanics, we need a high-precision input capture system. This change establishes the foundational layer: capturing raw keyboard and mouse events with sub-millisecond accuracy on Windows without injecting into game processes.

## What Changes
- **New Core Library (`NoteD.Core`)**:
  - `NativeMethods`: P/Invoke wrappers for `User32.dll` (Raw Input) and `Kernel32.dll` (QPC).
  - `HighResolutionTimer`: Timestamp provider using `QueryPerformanceCounter`.
  - `InputManager`: Handles Raw Input registration and message loop.
  - `CsvLogger`: High-performance, non-locking file logger.
- **New CLI Tool (`NoteD.Cli`)**:
  - A headless console application to verify input capture and logging (Step 1 MVP).

## Impact
- **Specs**:
  - `specs/input/spec.md`: Defines requirements for Raw Input and QPC.
  - `specs/logging/spec.md`: Defines CSV format and performance constraints.
- **Platform**: Windows 10/11 x64.

## Non-goals
- UI/Overlay (planned for `add-overlay-renderer`).
- Analysis/Metrics (planned for `add-input-analysis`).
- Linux/macOS support.


