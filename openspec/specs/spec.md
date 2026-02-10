# Spec: Input Capture

## Context
Capturing player input with high precision is critical for calculating deadzone and counter-strafing timings. We use Windows Raw Input API to avoid anti-cheat interference.

## Requirements

### Raw Input Capture
- **Must** use `RegisterRawInputDevices` with `RIDEV_INPUTSINK` to capture input while in background.
- **Must** capture Keyboard (`GenericDesktop` / `Keyboard`) and Mouse (`GenericDesktop` / `Mouse`).
- **Must** filter for specific keys (default: A, D, W, S, Arrow Keys).
- **Must** capture Mouse Left Button (Down/Up).

### High-Resolution Timing
- **Must** use `QueryPerformanceCounter` (QPC) for timestamps.
- **Must** convert QPC ticks to milliseconds using `QueryPerformanceFrequency`.
- **Must** provide resolution of at least 0.1ms.

## Scenarios

#### Scenario: Capturing a key press
Given the application is running
When the user presses the 'A' key
Then a `KeyDown` event is recorded with the 'A' key code
And the event has a valid QPC timestamp

#### Scenario: Capturing a mouse click
Given the application is running
When the user clicks the Left Mouse Button
Then a `MouseDown` event is recorded
And the event has a valid QPC timestamp


