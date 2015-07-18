using System;

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
			var timer = new HiPerfTimer();

			_mapper.Initialize();
			_mapper.Map();

			timer.Start();

			for (int i = 0; i < 1000000; i++)
			{
				_mapper.Map();
			}

			timer.Stop();

			Console.WriteLine("{0}: - {1} - Mapping time: \t{2}s", _mapper.Name, _mode, timer.Duration);
		}
	}
}