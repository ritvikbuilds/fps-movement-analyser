using Xunit;

namespace NoteD.Core.Tests;

public class DeltaCalculatorTests
{
    private static InputEvent CreateKeyEvent(double ms, string key, InputEventType type)
    {
        return new InputEvent(
            TimestampQpc: (long)(ms * 1000),
            TimestampMs: ms,
            Device: InputDeviceType.Keyboard,
            Key: key,
            Type: type
        );
    }

    private static InputEvent CreateClickEvent(double ms)
    {
        return new InputEvent(
            TimestampQpc: (long)(ms * 1000),
            TimestampMs: ms,
            Device: InputDeviceType.Mouse,
            Key: "MouseLeft",
            Type: InputEventType.Down
        );
    }

    [Fact]
    public void ComputeDeltas_DeadzoneOnly_HoldAndReleaseThenClick()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        
        buffer.Add(CreateKeyEvent(100.0, "A", InputEventType.Down));
        buffer.Add(CreateKeyEvent(150.0, "A", InputEventType.Up));
        
        var click = CreateClickEvent(165.0);
        buffer.Add(click);
        
        var result = calc.ComputeDeltas(click);
        
        Assert.NotNull(result.DeadzoneDeltaMs);
        Assert.Equal(15.0, result.DeadzoneDeltaMs.Value, precision: 1);
        Assert.Equal("A", result.LastKeyUp);
    }

    [Fact]
    public void ComputeDeltas_CounterstrafeOnly_OppositeKeyPressed()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        
        buffer.Add(CreateKeyEvent(100.0, "A", InputEventType.Down));
        buffer.Add(CreateKeyEvent(120.0, "D", InputEventType.Down));
        
        var click = CreateClickEvent(140.0);
        buffer.Add(click);
        
        var result = calc.ComputeDeltas(click);
        
        Assert.NotNull(result.CounterstrafeDeltaMs);
        Assert.Equal(20.0, result.CounterstrafeDeltaMs.Value, precision: 1);
        Assert.Equal("D", result.OppositeKeyDown);
    }

    [Fact]
    public void ComputeDeltas_BothDeltas_ReleaseAndCounterstrafe()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        
        buffer.Add(CreateKeyEvent(100.0, "A", InputEventType.Down));
        buffer.Add(CreateKeyEvent(130.0, "A", InputEventType.Up));
        buffer.Add(CreateKeyEvent(135.0, "D", InputEventType.Down));
        
        var click = CreateClickEvent(150.0);
        buffer.Add(click);
        
        var result = calc.ComputeDeltas(click);
        
        Assert.NotNull(result.DeadzoneDeltaMs);
        Assert.NotNull(result.CounterstrafeDeltaMs);
        Assert.Equal(20.0, result.DeadzoneDeltaMs.Value, precision: 1);
        Assert.Equal(15.0, result.CounterstrafeDeltaMs.Value, precision: 1);
    }

    [Fact]
    public void ComputeDeltas_NoDeltas_WhenNoRecentKeyEvents()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        
        var click = CreateClickEvent(1000.0);
        buffer.Add(click);
        
        var result = calc.ComputeDeltas(click);
        
        Assert.Null(result.DeadzoneDeltaMs);
        Assert.Null(result.CounterstrafeDeltaMs);
    }

    [Fact]
    public void ComputeDeltas_NoDeltas_WhenEventsOutsideWindow()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 50.0);
        
        buffer.Add(CreateKeyEvent(100.0, "A", InputEventType.Down));
        buffer.Add(CreateKeyEvent(120.0, "A", InputEventType.Up));
        
        var click = CreateClickEvent(200.0);
        buffer.Add(click);
        
        var result = calc.ComputeDeltas(click);
        
        Assert.Null(result.DeadzoneDeltaMs);
    }

    [Fact]
    public void ComputeDeltas_IgnoresNonMouseEvents()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        
        var keyEvent = CreateKeyEvent(100.0, "A", InputEventType.Down);
        var result = calc.ComputeDeltas(keyEvent);
        
        Assert.Null(result.DeadzoneDeltaMs);
        Assert.Null(result.CounterstrafeDeltaMs);
    }

    [Fact]
    public void ComputeDeltas_IgnoresMouseUp()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        
        buffer.Add(CreateKeyEvent(100.0, "A", InputEventType.Up));
        
        var mouseUp = new InputEvent(
            TimestampQpc: 150000,
            TimestampMs: 150.0,
            Device: InputDeviceType.Mouse,
            Key: "MouseLeft",
            Type: InputEventType.Up
        );
        
        var result = calc.ComputeDeltas(mouseUp);
        
        Assert.Null(result.DeadzoneDeltaMs);
    }

    [Fact]
    public void FormatDelta_FormatsCorrectly()
    {
        Assert.Equal("15.3 ms", DeltaCalculator.FormatDelta(15.34));
        Assert.Equal("0.5 ms", DeltaCalculator.FormatDelta(0.49));
        Assert.Equal("-", DeltaCalculator.FormatDelta(null));
    }

    [Fact]
    public void LookupWindowMs_CanBeChanged()
    {
        var buffer = new EventRingBuffer(100);
        var calc = new DeltaCalculator(buffer, lookupWindowMs: 100.0);
        
        Assert.Equal(100.0, calc.LookupWindowMs);
        
        calc.LookupWindowMs = 300.0;
        Assert.Equal(300.0, calc.LookupWindowMs);
    }
}


