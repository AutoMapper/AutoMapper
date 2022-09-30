namespace Benchmark;

public interface IObjectToObjectMapper
{
    string Name { get; }
    void Initialize();
    object Map();
}