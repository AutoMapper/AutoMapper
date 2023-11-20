using AutoMapper.Execution;

namespace AutoMapper.UnitTests;

public class CreateProxyThreading
{
    [Fact]
    public async Task Should_create_the_proxy_once()
    {
        var tasks = Enumerable.Range(0, 5).Select(i => Task.Factory.StartNew(() =>
        {
            ProxyGenerator.GetProxyType(typeof(ISomeDto));
        })).ToArray();
        await Task.WhenAll(tasks);
    }

    public interface ISomeDto
    {
        string Property1 { get; set; }
        string Property21 { get; set; }
        string Property3 { get; set; }
        string Property4 { get; set; }
        string Property5 { get; set; }
        string Property6 { get; set; }
        string Property7 { get; set; }
        string Property8 { get; set; }
        string Property9 { get; set; }
        string Property10 { get; set; }
        string Property11 { get; set; }
    }

}
