using System;

namespace Benchmark
{
	public interface IObjectToObjectMapper
	{
		string Name { get; }
		void Initialize();
		void Map();
	}
}