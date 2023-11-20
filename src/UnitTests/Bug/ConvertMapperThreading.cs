namespace AutoMapper.UnitTests;

public class ConvertMapperThreading
{
    class Source
    {
        public string Number { get; set; }
    }

    class Destination
    {
        public int Number { get; set; }
    }

    [Fact]
    public async Task Should_work()
    {
        var tasks = Enumerable.Range(0, 5).Select(i => Task.Factory.StartNew(() =>
        {
            new MapperConfiguration(c => c.CreateMap<Source, Destination>());
        })).ToArray();
        try
        {
            await Task.WhenAll(tasks);
        }
        catch(AggregateException ex)
        {
            ex.Handle(e =>
            {
                if(e is InvalidOperationException)
                {
                    throw e;
                }
                return false;
            });
        }
    }
}