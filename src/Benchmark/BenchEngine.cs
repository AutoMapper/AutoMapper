namespace Benchmark;

public class BenchEngine
{
    private readonly IObjectToObjectMapper _mapper;
    private readonly string _mode;

    public BenchEngine(IObjectToObjectMapper mapper, string mode)
    {
        _mapper = mapper;
        _mode = mode;
    }

    public void Start()
    {
        _mapper.Initialize();
        _mapper.Map();

        var timer = Stopwatch.StartNew();

        for(int i = 0; i < 1_000_000; i++)
        {
            _mapper.Map();
        }

        timer.Stop();

        Console.WriteLine("{2:D3} ms {0}: - {1}", _mapper.Name, _mode, (int)timer.Elapsed.TotalMilliseconds);
    }
}