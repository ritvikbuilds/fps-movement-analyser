namespace NoteD.Core;

public class EventRingBuffer
{
    private readonly InputEvent[] _buffer;
    private readonly object _lock = new();
    private int _head;
    private int _count;

    public EventRingBuffer(int capacity = 10000)
    {
        _buffer = new InputEvent[capacity];
        _head = 0;
        _count = 0;
    }

    public int Count
    {
        get { lock (_lock) return _count; }
    }

    public int Capacity => _buffer.Length;

    public void Add(InputEvent evt)
    {
        lock (_lock)
        {
            _buffer[_head] = evt;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length)
                _count++;
        }
    }

    public InputEvent? GetLatest(int offset = 0)
    {
        lock (_lock)
        {
            if (_count == 0 || offset >= _count) return null;
            int index = (_head - 1 - offset + _buffer.Length) % _buffer.Length;
            return _buffer[index];
        }
    }

    public List<InputEvent> GetRecent(int maxCount)
    {
        lock (_lock)
        {
            var result = new List<InputEvent>(Math.Min(maxCount, _count));
            for (int i = 0; i < Math.Min(maxCount, _count); i++)
            {
                int index = (_head - 1 - i + _buffer.Length) % _buffer.Length;
                result.Add(_buffer[index]);
            }
            return result;
        }
    }

    public List<InputEvent> GetEventsInWindow(double currentMs, double windowMs)
    {
        lock (_lock)
        {
            var result = new List<InputEvent>();
            double minMs = currentMs - windowMs;
            
            for (int i = 0; i < _count; i++)
            {
                int index = (_head - 1 - i + _buffer.Length) % _buffer.Length;
                var evt = _buffer[index];
                
                if (evt.TimestampMs < minMs)
                    break;
                    
                result.Add(evt);
            }
            return result;
        }
    }

    public InputEvent? FindLastKeyUp(double beforeMs, double windowMs, HashSet<string>? keys = null)
    {
        lock (_lock)
        {
            double minMs = beforeMs - windowMs;
            
            for (int i = 0; i < _count; i++)
            {
                int index = (_head - 1 - i + _buffer.Length) % _buffer.Length;
                var evt = _buffer[index];
                
                if (evt.TimestampMs >= beforeMs)
                    continue;
                if (evt.TimestampMs < minMs)
                    break;
                    
                if (evt.Device == InputDeviceType.Keyboard && 
                    evt.Type == InputEventType.Up &&
                    (keys == null || keys.Contains(evt.Key)))
                {
                    return evt;
                }
            }
            return null;
        }
    }

    public InputEvent? FindLastOppositeKeyDown(double beforeMs, double windowMs, string currentKey)
    {
        lock (_lock)
        {
            double minMs = beforeMs - windowMs;
            string? oppositeKey = GetOppositeKey(currentKey);
            if (oppositeKey == null) return null;
            
            for (int i = 0; i < _count; i++)
            {
                int index = (_head - 1 - i + _buffer.Length) % _buffer.Length;
                var evt = _buffer[index];
                
                if (evt.TimestampMs >= beforeMs)
                    continue;
                if (evt.TimestampMs < minMs)
                    break;
                    
                if (evt.Device == InputDeviceType.Keyboard && 
                    evt.Type == InputEventType.Down &&
                    evt.Key == oppositeKey)
                {
                    return evt;
                }
            }
            return null;
        }
    }

    public string? FindLastHeldKey(double beforeMs, double windowMs)
    {
        lock (_lock)
        {
            double minMs = beforeMs - windowMs;
            var keyStates = new Dictionary<string, bool>();
            
            for (int i = 0; i < _count; i++)
            {
                int index = (_head - 1 - i + _buffer.Length) % _buffer.Length;
                var evt = _buffer[index];
                
                if (evt.TimestampMs >= beforeMs)
                    continue;
                if (evt.TimestampMs < minMs)
                    break;
                    
                if (evt.Device == InputDeviceType.Keyboard)
                {
                    if (!keyStates.ContainsKey(evt.Key))
                    {
                        if (evt.Type == InputEventType.Down)
                            return evt.Key;
                        keyStates[evt.Key] = false;
                    }
                }
            }
            return null;
        }
    }

    private static string? GetOppositeKey(string key)
    {
        return key switch
        {
            "A" => "D",
            "D" => "A",
            "W" => "S",
            "S" => "W",
            "Left" => "Right",
            "Right" => "Left",
            "Up" => "Down",
            "Down" => "Up",
            _ => null
        };
    }

    public void Clear()
    {
        lock (_lock)
        {
            _head = 0;
            _count = 0;
        }
    }
}


