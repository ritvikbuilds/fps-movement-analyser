namespace NoteD.Core;

public record struct DeltaResult(
    double? DeadzoneDeltaMs,
    double? CounterstrafeDeltaMs,
    string? LastKeyUp,
    string? OppositeKeyDown
);

public class DeltaCalculator
{
    private readonly EventRingBuffer _buffer;
    private double _lookupWindowMs;
    private readonly HashSet<string> _monitoredKeys;

    public DeltaCalculator(EventRingBuffer buffer, double lookupWindowMs = 200.0, HashSet<string>? monitoredKeys = null)
    {
        _buffer = buffer;
        _lookupWindowMs = lookupWindowMs;
        _monitoredKeys = monitoredKeys ?? new HashSet<string> { "A", "D", "W", "S", "Left", "Right", "Up", "Down" };
    }

    public double LookupWindowMs
    {
        get => _lookupWindowMs;
        set => _lookupWindowMs = value;
    }

    public static string FormatDelta(double? deltaMs)
    {
        return deltaMs.HasValue ? $"{deltaMs.Value:0.0} ms" : "-";
    }

    public DeltaResult ComputeDeltas(InputEvent clickEvent)
    {
        if (clickEvent.Device != InputDeviceType.Mouse || clickEvent.Type != InputEventType.Down)
        {
            return new DeltaResult(null, null, null, null);
        }

        double clickMs = clickEvent.TimestampMs;
        
        var deadzoneResult = ComputeDeadzoneDelta(clickMs);
        var counterstrafeResult = ComputeCounterstrafeData(clickMs);

        return new DeltaResult(
            deadzoneResult.DeltaMs,
            counterstrafeResult.DeltaMs,
            deadzoneResult.Key,
            counterstrafeResult.Key
        );
    }

    public (double? DeltaMs, string? Key) ComputeDeadzoneDelta(double clickMs)
    {
        var lastKeyUp = _buffer.FindLastKeyUp(clickMs, _lookupWindowMs, _monitoredKeys);
        
        if (lastKeyUp == null)
        {
            return (null, null);
        }

        double delta = clickMs - lastKeyUp.Value.TimestampMs;
        return (delta, lastKeyUp.Value.Key);
    }

    public (double? DeltaMs, string? Key) ComputeCounterstrafeData(double clickMs)
    {
        string? lastHeldKey = _buffer.FindLastHeldKey(clickMs, _lookupWindowMs);
        
        if (lastHeldKey == null)
        {
            return (null, null);
        }

        var oppositeKeyDown = _buffer.FindLastOppositeKeyDown(clickMs, _lookupWindowMs, lastHeldKey);
        
        if (oppositeKeyDown == null)
        {
            return (null, null);
        }

        double delta = clickMs - oppositeKeyDown.Value.TimestampMs;
        return (delta, oppositeKeyDown.Value.Key);
    }
}
