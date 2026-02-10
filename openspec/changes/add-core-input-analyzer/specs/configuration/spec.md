# Configuration Specification

## ADDED Requirements

### REQ-CF-001: Settings Persistence
User settings MUST be persisted to disk and restored on application startup.

#### Scenario: Settings survive restart
Given the user changes overlay opacity to 75%
And the application is closed
When the application is restarted
Then overlay opacity is 75%

### REQ-CF-002: Configurable Monitored Keys
Users MUST be able to add/remove keys from the monitored set.

#### Scenario: Add key to monitor list
Given the default keys are A and D
When the user adds "W" to monitored keys
Then W key events are captured
And settings show A, D, W as monitored

#### Scenario: Remove key from monitor list
Given keys A, D, W are monitored
When the user removes "W"
Then W key events are no longer captured

### REQ-CF-003: Timing Configuration
Users MUST be able to configure lookup window (ms) and debounce threshold (ms).

#### Scenario: Adjust lookup window
Given the default lookup window is 200ms
When the user sets it to 300ms
Then delta computation searches within 300ms

#### Scenario: Adjust debounce threshold
Given debounce is set to 5ms
When two events occur within 3ms
Then the second event is filtered as switch chatter

### REQ-CF-004: Visual Configuration
Users MUST be able to configure overlay appearance settings.

#### Scenario: Configure colors
Given default colors are A=orange, D=blue
When the user changes A to green
Then A lane bars render as green

#### Scenario: Configure opacity
Given default opacity is 100%
When the user sets opacity to 50%
Then all overlay elements are 50% transparent

#### Scenario: Configure font size
Given default font size is 12pt
When the user sets font size to 16pt
Then all overlay text renders at 16pt

### REQ-CF-005: Hotkey Configuration
Users MUST be able to configure hotkeys for common actions.

#### Scenario: Configure logging hotkey
Given default logging hotkey is F9
When the user changes it to F10
Then pressing F10 toggles logging
And F9 no longer toggles logging

### REQ-CF-006: Output Path Configuration
Users MUST be able to configure the directory for log file output.

#### Scenario: Set output directory
Given default output is Documents/NoteD/
When the user sets output to D:/Recordings/NoteD/
Then new log files are created in D:/Recordings/NoteD/

### REQ-CF-007: Settings Window
A settings window MUST be accessible via hotkey or system tray icon.

#### Scenario: Open settings via hotkey
Given the application is running
When the user presses the settings hotkey (default: F8)
Then the settings window opens
And the overlay becomes interactive

### REQ-CF-008: Default Configuration
The application MUST have sensible defaults that work without configuration.

#### Scenario: First run with defaults
Given the application is launched for the first time
Then monitored keys are A and D
And lookup window is 200ms
And debounce is 2ms
And overlay opacity is 100%
And output directory is Documents/NoteD/
And logging hotkey is F9
And settings hotkey is F8


