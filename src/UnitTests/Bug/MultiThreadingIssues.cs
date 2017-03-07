using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.Mappers;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    using Should;

    public class MultiThreadingIssues
    {
        public class Type1
        {
            public string FirstName;
            public string MiddleName;
            public string LastName;
            public int Age;
        }

        public class Type1Point3
        {
            public string FirstName;
            public string MiddleName;
            public string LastName;
        }

        public class Type1Point2
        {
            public string FirstName;
            public string MiddleName;
        }

        public class Type1Point1
        {
            public string FirstName;
        }

        public class DestType
        {
            public string FirstName;
            public string MiddleName;
            public string LastName;
            public int Age;
        }

        static int _done;
        readonly ManualResetEvent _allDone = new ManualResetEvent(false);

        [Fact]
        public void ShouldMapToNewISet()
        {
            const int threadCount = 130;

            for(int i = 0; i < threadCount; i++)
            {
                Task.Factory.StartNew(doMapping).ContinueWith(
                    a =>
                    {
                        if(Interlocked.Increment(ref _done) == threadCount)
                        {
                            _allDone.Set();
                        }

                    });
            }

            _allDone.WaitOne(TimeSpan.FromSeconds(10));
        }

        static void doMapping()
        {
            var source = createSource();

            Console.WriteLine(@"Mapping {0} on thread {1}", source.GetType(), Thread.CurrentThread.ManagedThreadId);

            var config = new MapperConfiguration(cfg => cfg.CreateMap(source.GetType(), typeof(DestType)));

            DestType t2 = (DestType)config.CreateMapper().Map(source, source.GetType(), typeof(DestType));
        }

        static readonly Random _random = new Random();

        static object createSource()
        {

            int n = _random.Next(0, 4);

            if(n == 0)
            {
                return new Type1
                {
                    Age = 12,
                    FirstName = @"Fred",
                    LastName = @"Smith",
                    MiddleName = @"G"
                };
            }
            if(n == 1)
            {
                return new Type1Point1()
                {
                    FirstName = @"Fred",
                };

            }
            if(n == 2)
            {
                return new Type1Point2()
                {
                    FirstName = @"Fred",
                    MiddleName = @"G"
                };

            }
            if(n == 3)
            {
                return new Type1Point3()
                {
                    FirstName = @"Fred",
                    LastName = @"Smith",
                    MiddleName = @"G"
                };

            }

            throw new Exception();
        }
    }

    public class DynamicMapThreadingIssues
    {
        public class SomeDtoA
        {
            public string Property1 { get; set; }
            public string Property21 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
        }

        public class SomeDtoB
        {
            public string Property1 { get; set; }
            public string Property21 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
        }

        public class SomeDtoC
        {
            public string Property1 { get; set; }
            public string Property21 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
        }

        public class SomeDtoD
        {
            public string Property1 { get; set; }
            public string Property21 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
        }

        [Fact]
        public void Should_not_fail()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
            var mapper = config.CreateMapper();

            var tasks = Enumerable.Range(0, 5).Select(index =>
              Task.Factory.StartNew(() =>
                  {
                      if(index % 2 == 0)
                      {
                          mapper.Map<SomeDtoA, SomeDtoB>(new SomeDtoA());
                          mapper.Map<SomeDtoC, SomeDtoD>(new SomeDtoC());
                      }
                      else
                      {
                          mapper.Map<SomeDtoB, SomeDtoA>(new SomeDtoB());
                          mapper.Map<SomeDtoD, SomeDtoC>(new SomeDtoD());
                      }
                  })).ToArray();
            Task.WaitAll(tasks);
        }
    }

    public class ResolveTypeMapThreadingIssues
    {
        public class SomeDtoA
        {
            public string Property1 { get; set; }
            public string Property21 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
        }

        public class SomeDtoB
        {
            public string Property1 { get; set; }
            public string Property21 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
        }

        public class SomeDtoC : SomeDtoA
        {
        }

        public class SomeDtoD : SomeDtoB
        {
        }

        [Fact]
        public void Should_not_fail()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
            var mapper = config.CreateMapper();

            var tasks = Enumerable.Range(0, 5).Select(index =>
              Task.Factory.StartNew(() =>
              {
                  if(index % 2 == 0)
                  {
                      mapper.Map<SomeDtoA, SomeDtoB>(new SomeDtoC());
                  }
                  else
                  {
                      mapper.Map<SomeDtoC, SomeDtoB>(new SomeDtoC());
                  }
              })).ToArray();
            Task.WaitAll(tasks);
        }
    }

}