using System.Runtime.InteropServices;
using NoteD.Core;

namespace NoteD.Cli;

class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AllocConsole();
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AttachConsole(int dwProcessId);
    
    const int ATTACH_PARENT_PROCESS = -1;

    static void Main(string[] args)
    {
        // Attach to parent console or create new one (for WinExe)
        if (!AttachConsole(ATTACH_PARENT_PROCESS))
        {
            AllocConsole();
        }
        
        Console.WriteLine("NoteD - FPS Input Analyzer v1.0.0");
        Console.WriteLine("==================================");
        
        string logFile = args.Length > 0 ? args[0] : $"noted_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        Console.WriteLine($"Logging to: {Path.GetFullPath(logFile)}");
        
        var buffer = new EventRingBuffer(10000);
        var deltaCalc = new DeltaCalculator(buffer, lookupWindowMs: 200.0);
        using var logger = new CsvEventLogger(logFile);
        
        using var listener = new RawInputListener((evt) =>
        {
            buffer.Add(evt);
            
            DeltaResult? deltas = null;
            if (evt.Device == InputDeviceType.Mouse && evt.Type == InputEventType.Down)
            {
                deltas = deltaCalc.ComputeDeltas(evt);
            }
            
            logger.Log(evt, deltas);
            
            string deltaInfo = "";
            if (deltas.HasValue)
            {
                var d = deltas.Value;
                if (d.DeadzoneDeltaMs.HasValue)
                    deltaInfo += $" [deadzone: {d.DeadzoneDeltaMs.Value:0.0}ms]";
                if (d.CounterstrafeDeltaMs.HasValue)
                    deltaInfo += $" [counter: {d.CounterstrafeDeltaMs.Value:0.0}ms]";
            }
            
            Console.WriteLine($"[{evt.TimestampMs:0.0}ms] {evt.Device,-8} {evt.Key,-10} {evt.Type,-4}{deltaInfo}");
        });

        try
        {
            Console.WriteLine("\nStarting Input Listener...");
            listener.Start();
            
            Console.WriteLine("Monitoring: W, A, S, D, Arrows, MouseLeft");
            Console.WriteLine("Press ENTER to stop.\n");
            
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("\nStopping... Events logged to CSV.");
        }
    }
}


