using System;

namespace Benchmark
{
	public class BenchEngine
	{
		private readonly IBenchmarker _benchmarker;
		private readonly string _mode;

		public BenchEngine(IBenchmarker benchmarker, string mode)
		{
			_benchmarker = benchmarker;
			_mode = mode;
		}

		public void Start()
		{
			var timer = new HiPerfTimer();

			_benchmarker.Initialize();
			_benchmarker.Execute();

			timer.Start();

			for (int i = 0; i < _benchmarker.Iterations; i++)
			{
				_benchmarker.Execute();
			}

			timer.Stop();

			Console.WriteLine("{0}: - {1} - Execution time: \t{2}s", _benchmarker.Name, _mode, timer.Duration);
		}
	}
}