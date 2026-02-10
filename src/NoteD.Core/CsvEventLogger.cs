namespace NoteD.Core;

public class CsvEventLogger : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly object _lock = new();
    private bool _disposed;

    public CsvEventLogger(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(filePath, append: false);
        WriteHeader();
    }

    private void WriteHeader()
    {
        _writer.WriteLine("timestamp_qpc,timestamp_ms,device,key,event_type,deadzone_delta_ms,counter_delta_ms");
        _writer.Flush();
    }

    public void Log(InputEvent evt, DeltaResult? deltas = null)
    {
        if (_disposed) return;

        lock (_lock)
        {
            string device = evt.Device.ToString().ToLowerInvariant();
            string eventType = evt.Type == InputEventType.Down ? "down" : "up";
            
            string deadzoneStr = deltas?.DeadzoneDeltaMs.HasValue == true 
                ? deltas.Value.DeadzoneDeltaMs!.Value.ToString("0.0") 
                : "";
            string counterStr = deltas?.CounterstrafeDeltaMs.HasValue == true 
                ? deltas.Value.CounterstrafeDeltaMs!.Value.ToString("0.0") 
                : "";

            _writer.WriteLine($"{evt.TimestampQpc},{evt.TimestampMs:0.0},{device},{evt.Key},{eventType},{deadzoneStr},{counterStr}");
            _writer.Flush();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        lock (_lock)
        {
            _writer.Dispose();
        }
    }
}
