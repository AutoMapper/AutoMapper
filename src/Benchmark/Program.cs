using System.Collections.Generic;
using Benchmark.Flattening;

namespace Benchmark
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var mappers = new Dictionary<string, IBenchmarker[]>
				{
					{ "Flattening", new IBenchmarker[] { new FlatteningMapper(), new ManualMapper() } },
					{ "Ctors", new IBenchmarker[] { new CtorMapper(), new ManualCtorMapper() } },
					{ "Aliases", new IBenchmarker[] { new CreateMap() } }
				};
		
			foreach (var pair in mappers)
			{
				foreach (var mapper in pair.Value)
				{
					new BenchEngine(mapper, pair.Key).Start();
				}
			}
		}
	}
}
