using System;

namespace Benchmark
{
	public interface IBenchmarker
	{
		string Name { get; }

		void Initialize();

		void Execute();

		int Iterations { get; }
	}
}