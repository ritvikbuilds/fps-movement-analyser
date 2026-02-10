namespace NoteD.Core;

public static class HighResolutionTimer
{
    private static readonly long _frequency;
    private static readonly double _frequencyDouble;

    static HighResolutionTimer()
    {
        if (!NativeMethods.QueryPerformanceFrequency(out _frequency))
        {
            throw new PlatformNotSupportedException("High resolution timer not supported on this system.");
        }
        _frequencyDouble = (double)_frequency;
    }

    public static long GetTimestamp()
    {
        NativeMethods.QueryPerformanceCounter(out long timestamp);
        return timestamp;
    }

    public static double ToMilliseconds(long timestamp)
    {
        // returns ms
        return (timestamp * 1000.0) / _frequencyDouble;
    }

    public static double ToMillisecondsDelta(long start, long end)
    {
        return ((end - start) * 1000.0) / _frequencyDouble;
    }
}


