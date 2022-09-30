using System.Runtime.InteropServices;

namespace Benchmark;

public class HiPerfTimer
{
    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceCounter(
        out long lpPerformanceCount);

    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(
        out long lpFrequency);

    private long _startTime, _stopTime;
    private long _freq;

    // Constructor
    public HiPerfTimer()
    {
        _startTime = 0;
        _stopTime = 0;

        if (QueryPerformanceFrequency(out _freq) == false)
        {
            // high-performance counter not supported
            throw new Win32Exception();
        }
    }

    // Start the timer
    public void Start()
    {
        // lets do the waiting threads there work
        Thread.Sleep(0);

        QueryPerformanceCounter(out _startTime);
    }

    // Stop the timer
    public void Stop()
    {
        QueryPerformanceCounter(out _stopTime);
    }

    // Returns the duration of the timer (in seconds)
    public double Duration
    {
        get
        {
            double d = (_stopTime - _startTime);
            return d / _freq;
        }
    }
}