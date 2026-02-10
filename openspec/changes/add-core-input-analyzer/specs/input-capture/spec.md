# Input Capture Specification

## ADDED Requirements

### REQ-IC-001: Raw Input Registration
The system MUST register for Raw Input events using `RegisterRawInputDevices` for both keyboard and mouse devices.

#### Scenario: Application startup registers for raw input
Given the application is starting
When the main window is created
Then the system registers for keyboard raw input (RIDEV_INPUTSINK flag)
And the system registers for mouse raw input (RIDEV_INPUTSINK flag)
And registration succeeds without error

### REQ-IC-002: Keyboard Event Capture
The system MUST capture keyboard keydown and keyup events for configured keys using `WM_INPUT` messages.

#### Scenario: A key press is captured
Given the user has configured key "A" for monitoring
When the user presses the "A" key
Then the system captures a keydown event with key="A" and type="down"
And the event includes a QPC timestamp

#### Scenario: A key release is captured
Given the user has configured key "A" for monitoring
When the user releases the "A" key
Then the system captures a keyup event with key="A" and type="up"
And the event includes a QPC timestamp

### REQ-IC-003: Mouse Click Capture
The system MUST capture mouse left-button down events using Raw Input.

#### Scenario: Mouse click is captured
Given mouse monitoring is enabled
When the user clicks the left mouse button
Then the system captures a click event with device="mouse", key="Mouse1", type="click"
And the event includes a QPC timestamp

### REQ-IC-004: High-Resolution Timestamps
The system MUST use `QueryPerformanceCounter` for all event timestamps, providing sub-millisecond accuracy.

#### Scenario: Timestamp precision is sub-millisecond
Given the system captures two events 500 microseconds apart
When the timestamps are compared
Then the difference is measurable and accurate to within 100 microseconds

### REQ-IC-005: Configurable Key Set
The system MUST allow users to configure which keys are monitored, defaulting to A and D.

#### Scenario: Default keys are A and D
Given the application starts with default settings
When a user presses A, D, W, S keys
Then only A and D key events are captured
And W and S key events are ignored

#### Scenario: User configures additional keys
Given the user adds "W" and "S" to monitored keys
When the user presses W, A, S, D keys
Then all four key events are captured

### REQ-IC-006: Event Data Structure
Each captured event MUST contain: device type, key identifier, event type, QPC ticks, and milliseconds.

#### Scenario: Event structure is complete
Given a keydown event is captured
Then the event contains device="keyboard"
And the event contains key="A" (or other configured key)
And the event contains type="down"
And the event contains qpc_ticks as 64-bit integer
And the event contains ms as double with sub-ms precision

### REQ-IC-007: Background Capture (INPUTSINK)
The system MUST capture input events even when the application window is not focused.

#### Scenario: Capture works while game is focused
Given the overlay application is running
And a game window has focus
When the user presses configured keys
Then all key events are captured with accurate timestamps

### REQ-IC-008: No Input Injection
The system MUST NOT synthesize, inject, or modify any input events. Capture is read-only.

#### Scenario: System only reads input
Given the application is running
When any input is captured
Then no SendInput, keybd_event, or mouse_event calls are made
And no input is blocked, modified, or synthesized


