# Delta Computation Specification

## ADDED Requirements

### REQ-DC-001: Deadzone Delta Calculation
For each mouse click, the system MUST compute `deadzone_delta_ms` as the time between the click and the most recent keyup event.

#### Scenario: Deadzone delta for release-then-click
Given the user releases key "A" at time T1
And the user clicks at time T2 (within 200ms of T1)
When the delta is computed
Then deadzone_delta_ms = T2 - T1
And the value is positive

#### Scenario: No recent keyup
Given no keyup event occurred in the last 200ms
When the user clicks
Then deadzone_delta_ms is null/undefined

### REQ-DC-002: Counterstrafe Delta Calculation
For each mouse click, the system MUST compute `counter_delta_ms` as the time between the click and the most recent opposite direction keydown.

#### Scenario: Counterstrafe delta for opposite key press
Given the user was holding "A"
And the user presses "D" at time T1 (counterstrafe)
And the user clicks at time T2 (within 200ms of T1)
When the delta is computed
Then counter_delta_ms = T2 - T1
And the value is positive

#### Scenario: Counterstrafe with A after D
Given the user was holding "D"
And the user presses "A" at time T1
And the user clicks at time T2
When the delta is computed
Then counter_delta_ms = T2 - T1

#### Scenario: No opposite keydown
Given only "A" was pressed (no "D" press in window)
When the user clicks
Then counter_delta_ms is null/undefined

### REQ-DC-003: Configurable Lookup Window
The system MUST use a configurable time window (default 200ms) for searching relevant prior events.

#### Scenario: Event outside window is ignored
Given the lookup window is 200ms
And the last keyup was 250ms ago
When the user clicks
Then deadzone_delta_ms is null (keyup too old)

#### Scenario: Custom window is respected
Given the user sets lookup window to 500ms
And the last keyup was 300ms ago
When the user clicks
Then deadzone_delta_ms is computed (within window)

### REQ-DC-004: Millisecond Display Precision
Delta values MUST be displayed with one decimal place (e.g., "16.2 ms").

#### Scenario: Delta formatting
Given a computed delta of 16.234 ms
When the delta is formatted for display
Then the output is "16.2 ms"

### REQ-DC-005: Delta Priority Logic
When both deltas are present, the system MUST determine which to highlight based on configuration (default: deadzone).

#### Scenario: Both deltas present, deadzone highlighted
Given deadzone_delta_ms = 15.0
And counter_delta_ms = 25.0
And highlight mode is "deadzone" (default)
When the click label is rendered
Then the primary label shows "15.0 ms"
And the secondary label optionally shows counter delta

### REQ-DC-006: Ring Buffer Event Search
Delta computation MUST search backwards through the event ring buffer efficiently.

#### Scenario: Search finds most recent relevant event
Given events: A_down(T1), A_up(T2), D_down(T3), click(T4)
When computing deltas for click at T4
Then deadzone uses A_up at T2
And counter uses D_down at T3 (opposite of last held key)


