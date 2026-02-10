using Xunit;

namespace NoteD.Core.Tests;

public class HighResolutionTimerTests
{
    [Fact]
    public void GetTimestamp_ReturnsPositiveValue()
    {
        long timestamp = HighResolutionTimer.GetTimestamp();
        Assert.True(timestamp > 0);
    }

    [Fact]
    public void GetTimestamp_IsMonotonic()
    {
        long t1 = HighResolutionTimer.GetTimestamp();
        long t2 = HighResolutionTimer.GetTimestamp();
        Assert.True(t2 >= t1);
    }

    [Fact]
    public void ToMilliseconds_ReturnsPositiveValue()
    {
        long timestamp = HighResolutionTimer.GetTimestamp();
        double ms = HighResolutionTimer.ToMilliseconds(timestamp);
        Assert.True(ms > 0);
    }

    [Fact]
    public void ToMillisecondsDelta_CalculatesCorrectDifference()
    {
        long start = HighResolutionTimer.GetTimestamp();
        Thread.Sleep(10);
        long end = HighResolutionTimer.GetTimestamp();
        
        double deltaMs = HighResolutionTimer.ToMillisecondsDelta(start, end);
        
        Assert.True(deltaMs >= 9.0, $"Delta was {deltaMs}ms, expected >= 9ms");
        Assert.True(deltaMs < 50.0, $"Delta was {deltaMs}ms, expected < 50ms");
    }

    [Fact]
    public void ToMillisecondsDelta_ZeroDeltaForSameTimestamp()
    {
        long timestamp = HighResolutionTimer.GetTimestamp();
        double deltaMs = HighResolutionTimer.ToMillisecondsDelta(timestamp, timestamp);
        Assert.Equal(0.0, deltaMs);
    }
}


