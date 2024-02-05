using Benchmark.Flattening;

namespace Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        var mappers = new Dictionary<string, IObjectToObjectMapper[]>
            {
                { "Flattening", new IObjectToObjectMapper[] { new FlatteningMapper() , new ManualMapper(), } },
                { "Ctors", new IObjectToObjectMapper[] { new CtorMapper(), new ManualCtorMapper(),  } },
                { "Complex", new IObjectToObjectMapper[] { new ComplexTypeMapper(), new ManualComplexTypeMapper() } },
                { "Deep", new IObjectToObjectMapper[] { new DeepTypeMapper(), new ManualDeepTypeMapper() } }
            };
        while (true)
        {
            foreach (var pair in mappers)
            {
                foreach (var mapper in pair.Value)
                {
                    new BenchEngine(mapper, pair.Key).Start();
                }
            }
            Console.ReadLine();
        }
    }
}
