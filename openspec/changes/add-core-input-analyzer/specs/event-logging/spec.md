# Event Logging Specification

## ADDED Requirements

### REQ-EL-001: Real-Time CSV Logging
The system MUST log events to CSV in real-time with configurable file path.

#### Scenario: Events logged to CSV
Given CSV logging is enabled
When keyboard and mouse events occur
Then each event is appended to the CSV file
And the file is flushed frequently to prevent data loss

### REQ-EL-002: CSV Format
CSV files MUST include header and required columns for analysis.

#### Scenario: CSV has correct format
Given a logging session has events
When the CSV is examined
Then the header row contains: timestamp_qpc,timestamp_ms,device,key,event_type,deadzone_delta_ms,counter_delta_ms
And each event row contains values for all columns
And delta columns are empty when not applicable

### REQ-EL-003: JSON Export Option
The system MUST support JSON export for full event data.

#### Scenario: JSON export includes all data
Given the user requests JSON export
When the export completes
Then a JSON file contains an array of event objects
And each object includes all event properties

### REQ-EL-004: Logging Toggle
Users MUST be able to start/stop logging via hotkey or UI button.

#### Scenario: Hotkey starts logging
Given logging is stopped
When the user presses the logging hotkey (default: F9)
Then logging starts
And a new CSV file is created with timestamp in filename

#### Scenario: Hotkey stops logging
Given logging is active
When the user presses the logging hotkey
Then logging stops
And the CSV file is finalized

### REQ-EL-005: Session File Naming
Log files MUST be automatically named with session timestamp.

#### Scenario: File naming convention
Given logging starts at 2024-01-15 14:30:45
When the file is created
Then the filename is "noted_2024-01-15_14-30-45.csv"
And the file is created in the configured output directory

### REQ-EL-006: Ring Buffer Event Retention
The in-memory ring buffer MUST retain the last N seconds of events (configurable, default 30s).

#### Scenario: Buffer retains configured duration
Given buffer retention is set to 30 seconds
And events have been occurring for 60 seconds
When the buffer is queried
Then only events from the last 30 seconds are available
And older events have been discarded


