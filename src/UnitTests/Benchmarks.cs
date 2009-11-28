using Benchmark;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class Benchmarks : NonValidatingSpecBase
	{
		[Test, Explicit]
		public void Run()
		{
			Program.Main(new string[0]);
		}
	}
}