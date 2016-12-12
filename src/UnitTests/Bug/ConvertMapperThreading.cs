using System;
using Xunit;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace AutoMapper.UnitTests
{
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
        public void Should_work()
        {
            var tasks = Enumerable.Range(0, 5).Select(i => Task.Factory.StartNew(() =>
            {
                new MapperConfiguration(c => c.CreateMap<Source, Destination>());
            })).ToArray();
            try
            {
                Task.WaitAll(tasks);
            }
            catch(AggregateException ex)
            {
                ex.Handle(e =>
                {
                    if(e is InvalidOperationException)
                    {
                        ExceptionDispatchInfo.Capture(e).Throw();
                    }
                    return false;
                });
            }
        }
    }
}