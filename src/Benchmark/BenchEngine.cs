using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Profiler;

namespace Benchmark
{
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

            DataCollection.CommentMarkProfile(1, "BEGIN " + _mapper);

            var timer = Stopwatch.StartNew();

            for(int i = 0; i < 1000000; i++)
            {
                _mapper.Map();
            }

            timer.Stop();

            DataCollection.CommentMarkProfile(1, "END " + _mapper);

            Console.WriteLine("{0}: - {1} - Mapping time: \t{2}s", _mapper.Name, _mode, timer.Elapsed.TotalSeconds);
        }
    }
}