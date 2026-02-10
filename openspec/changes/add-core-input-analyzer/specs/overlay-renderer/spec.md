# Overlay Renderer Specification

## ADDED Requirements

### REQ-OR-001: Transparent Always-On-Top Window
The overlay MUST be a transparent, always-on-top window that can be captured by OBS.

#### Scenario: Overlay is visible over game
Given the game is running fullscreen
When the overlay is enabled
Then the overlay appears above the game
And the overlay background is transparent
And OBS Window Capture can see the overlay

### REQ-OR-002: Click-Through Mode
The overlay MUST be click-through by default so it doesn't block game input.

#### Scenario: Clicks pass through overlay
Given the overlay is in click-through mode
When the user clicks on the overlay area
Then the click passes through to the game
And the game receives the input

#### Scenario: Toggle interactive mode
Given the user presses the settings hotkey
When interactive mode is enabled
Then clicks on the overlay interact with overlay UI
And the game does not receive those clicks

### REQ-OR-003: Key Lane Visualization
The overlay MUST display horizontal lanes for each monitored key showing hold duration as colored bars.

#### Scenario: A key held shows orange bar
Given key "A" is being held
When the overlay renders
Then an orange horizontal bar appears in the A lane
And the bar length represents hold duration

#### Scenario: D key held shows blue bar
Given key "D" is being held
When the overlay renders
Then a blue horizontal bar appears in the D lane

#### Scenario: Key released ends bar
Given key "A" was held and is now released
When the overlay renders
Then the orange bar stops growing
And the bar remains visible in the timeline history

### REQ-OR-004: Mouse Click Visualization
Mouse clicks MUST be displayed as red dots at the corresponding time position with a millisecond label.

#### Scenario: Click appears as red dot
Given a mouse click event occurs
When the overlay renders
Then a small red circle appears at the click time position
And a label shows the delta (e.g., "16.2 ms") above the dot

### REQ-OR-005: Timeline Scrolling
The overlay MUST show a configurable time window (default 5 seconds) with older events scrolling off.

#### Scenario: Timeline shows last 5 seconds
Given 10 seconds of events have occurred
When the overlay renders
Then only the last 5 seconds of events are visible
And older events have scrolled off the left edge

#### Scenario: Time axis with markers
Given the timeline is rendering
When the overlay draws
Then time markers appear at the bottom (0s, 1s, 2s, etc.)

### REQ-OR-006: GPU-Accelerated Rendering
The overlay MUST use hardware-accelerated rendering (WPF/Direct2D) at 60-120 FPS.

#### Scenario: Smooth rendering under load
Given rapid input events are occurring
When the overlay renders
Then frame rate remains above 60 FPS
And CPU usage stays below 5%

### REQ-OR-007: Label Styling
Delta labels MUST use white text on a dark rounded background for readability.

#### Scenario: Label is readable over any background
Given a click delta label is displayed
When the label renders
Then text is white
And background is dark/semi-transparent rounded rectangle
And text shows one decimal place (e.g., "16.2 ms")

### REQ-OR-008: Configurable Appearance
Users MUST be able to configure overlay opacity, colors, font size, and position.

#### Scenario: User changes overlay opacity
Given the user sets opacity to 80%
When the overlay renders
Then all overlay elements are 80% opaque

#### Scenario: User changes key colors
Given the user sets A lane color to green
When the A key is held
Then the bar is green instead of orange

