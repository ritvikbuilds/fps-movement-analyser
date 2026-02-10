using Xunit;

namespace NoteD.Core.Tests;

public class EventRingBufferTests
{
    private static InputEvent CreateEvent(double ms, string key, InputEventType type, InputDeviceType device = InputDeviceType.Keyboard)
    {
        return new InputEvent(
            TimestampQpc: (long)(ms * 1000),
            TimestampMs: ms,
            Device: device,
            Key: key,
            Type: type
        );
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var buffer = new EventRingBuffer(100);
        Assert.Equal(0, buffer.Count);
        
        buffer.Add(CreateEvent(1.0, "A", InputEventType.Down));
        Assert.Equal(1, buffer.Count);
        
        buffer.Add(CreateEvent(2.0, "A", InputEventType.Up));
        Assert.Equal(2, buffer.Count);
    }

    [Fact]
    public void Add_WrapsAroundWhenFull()
    {
        var buffer = new EventRingBuffer(3);
        
        buffer.Add(CreateEvent(1.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(2.0, "A", InputEventType.Up));
        buffer.Add(CreateEvent(3.0, "D", InputEventType.Down));
        buffer.Add(CreateEvent(4.0, "D", InputEventType.Up));
        
        Assert.Equal(3, buffer.Count);
        
        var latest = buffer.GetLatest();
        Assert.NotNull(latest);
        Assert.Equal(4.0, latest.Value.TimestampMs);
    }

    [Fact]
    public void GetLatest_ReturnsNullWhenEmpty()
    {
        var buffer = new EventRingBuffer(100);
        Assert.Null(buffer.GetLatest());
    }

    [Fact]
    public void GetLatest_ReturnsCorrectEvent()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(1.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(2.0, "D", InputEventType.Down));
        
        var latest = buffer.GetLatest();
        Assert.NotNull(latest);
        Assert.Equal("D", latest.Value.Key);
        Assert.Equal(2.0, latest.Value.TimestampMs);
    }

    [Fact]
    public void GetLatest_WithOffset_ReturnsCorrectEvent()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(1.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(2.0, "D", InputEventType.Down));
        buffer.Add(CreateEvent(3.0, "A", InputEventType.Up));
        
        var evt = buffer.GetLatest(offset: 1);
        Assert.NotNull(evt);
        Assert.Equal("D", evt.Value.Key);
    }

    [Fact]
    public void GetRecent_ReturnsEventsInReverseOrder()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(1.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(2.0, "D", InputEventType.Down));
        buffer.Add(CreateEvent(3.0, "A", InputEventType.Up));
        
        var recent = buffer.GetRecent(3);
        
        Assert.Equal(3, recent.Count);
        Assert.Equal(3.0, recent[0].TimestampMs);
        Assert.Equal(2.0, recent[1].TimestampMs);
        Assert.Equal(1.0, recent[2].TimestampMs);
    }

    [Fact]
    public void FindLastKeyUp_FindsCorrectEvent()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(100.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(150.0, "A", InputEventType.Up));
        buffer.Add(CreateEvent(160.0, "D", InputEventType.Down));
        
        var keyUp = buffer.FindLastKeyUp(beforeMs: 170.0, windowMs: 200.0);
        
        Assert.NotNull(keyUp);
        Assert.Equal("A", keyUp.Value.Key);
        Assert.Equal(InputEventType.Up, keyUp.Value.Type);
    }

    [Fact]
    public void FindLastKeyUp_ReturnsNullWhenOutsideWindow()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(100.0, "A", InputEventType.Up));
        
        var keyUp = buffer.FindLastKeyUp(beforeMs: 400.0, windowMs: 100.0);
        
        Assert.Null(keyUp);
    }

    [Fact]
    public void FindLastOppositeKeyDown_FindsOppositeKey()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(100.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(150.0, "D", InputEventType.Down));
        
        var opposite = buffer.FindLastOppositeKeyDown(beforeMs: 160.0, windowMs: 200.0, currentKey: "A");
        
        Assert.NotNull(opposite);
        Assert.Equal("D", opposite.Value.Key);
    }

    [Fact]
    public void Clear_ResetsBuffer()
    {
        var buffer = new EventRingBuffer(100);
        buffer.Add(CreateEvent(1.0, "A", InputEventType.Down));
        buffer.Add(CreateEvent(2.0, "D", InputEventType.Down));
        
        buffer.Clear();
        
        Assert.Equal(0, buffer.Count);
        Assert.Null(buffer.GetLatest());
    }
}


