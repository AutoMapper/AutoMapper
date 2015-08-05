#if !SILVERLIGHT && !NETFX_CORE
using System ;
using System.Collections.Generic;
using System.Diagnostics ;
using System.Linq;
using System.Threading ;
using System.Threading.Tasks ;
using AutoMapper.Mappers;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    using Should;

    public class MultiThreadingIssues
    {
		public class Type1
		{
			public string FirstName ;
			public string MiddleName ;
			public string LastName ;
			public int Age ;
		}

		public class Type1Point3
		{
			public string FirstName ;
			public string MiddleName ;
			public string LastName ;
		}

		public class Type1Point2
		{
			public string FirstName ;
			public string MiddleName ;
		}

		public class Type1Point1
		{
			public string FirstName ;
		}

		public class DestType
		{
			public string FirstName ;
			public string MiddleName ;
			public string LastName ;
			public int Age ;
		}

    	static int _done ;
    	readonly ManualResetEvent _allDone = new ManualResetEvent( false );

		[Fact]
        public void ShouldMapToNewISet()
        {
			const int threadCount = 130 ;

			for (int i = 0; i < threadCount; i++)
			{
				Task.Factory.StartNew( doMapping ).ContinueWith(
					a =>
						{
							if (Interlocked.Increment(ref _done) == threadCount)
							{
								_allDone.Set( ) ;
							}

						} ) ;
			}

			_allDone.WaitOne( TimeSpan.FromSeconds( 10 ) ) ;
        }

    	static void doMapping( )
    	{
    		var source = createSource( ) ;

    		Console.WriteLine( @"Mapping {0} on thread {1}", source.GetType( ), Thread.CurrentThread.ManagedThreadId ) ;

    		Mapper.CreateMap( source.GetType( ), typeof( DestType ) ) ;

    		DestType t2 = (DestType)Mapper.Map(source, source.GetType(  ), typeof( DestType ) )  ;
    	}
    	
		static readonly Random _random = new Random();

    	static object createSource( )
    	{
    		
			int n = _random.Next( 0, 4 ) ;
    		
			if( n == 0 )
			{
				return new Type1
					{
						Age = 12,
						FirstName = @"Fred",
						LastName = @"Smith",
						MiddleName = @"G"
					} ;
			}
    		if( n == 1 )
    		{
    			return new Type1Point1( ) 
					{
						FirstName = @"Fred",
					} ;

    		}
    		if( n == 2 )
    		{
    			return new Type1Point2( ) 
					{
						FirstName = @"Fred",
						MiddleName = @"G"
					} ;

    		}
    		if( n == 3 )
    		{
    			return new Type1Point3( ) 
					{
						FirstName = @"Fred",
						LastName = @"Smith",
						MiddleName = @"G"
					} ;

    		}
    		
			throw new Exception();
    	}
    }

    public class DynamicMapThreadingIssues
    {
        public class SomeDtoA
        {
            private string Property1 { get; set; }
            private string Property21 { get; set; }
            private string Property3 { get; set; }
            private string Property4 { get; set; }
            private string Property5 { get; set; }
            private string Property6 { get; set; }
            private string Property7 { get; set; }
            private string Property8 { get; set; }
            private string Property9 { get; set; }
            private string Property10 { get; set; }
            private string Property11 { get; set; }
        }

        public class SomeDtoB
        {
            private string Property1 { get; set; }
            private string Property21 { get; set; }
            private string Property3 { get; set; }
            private string Property4 { get; set; }
            private string Property5 { get; set; }
            private string Property6 { get; set; }
            private string Property7 { get; set; }
            private string Property8 { get; set; }
            private string Property9 { get; set; }
            private string Property10 { get; set; }
            private string Property11 { get; set; }
        }

        public class SomeDtoC
        {
            private string Property1 { get; set; }
            private string Property21 { get; set; }
            private string Property3 { get; set; }
            private string Property4 { get; set; }
            private string Property5 { get; set; }
            private string Property6 { get; set; }
            private string Property7 { get; set; }
            private string Property8 { get; set; }
            private string Property9 { get; set; }
            private string Property10 { get; set; }
            private string Property11 { get; set; }
        }

        public class SomeDtoD
        {
            private string Property1 { get; set; }
            private string Property21 { get; set; }
            private string Property3 { get; set; }
            private string Property4 { get; set; }
            private string Property5 { get; set; }
            private string Property6 { get; set; }
            private string Property7 { get; set; }
            private string Property8 { get; set; }
            private string Property9 { get; set; }
            private string Property10 { get; set; }
            private string Property11 { get; set; }
        }

        [Fact]
        public void Should_not_fail()
        {
            Mapper.Reset();

            var tasks = Enumerable.Range(0, 5).Select(
          i =>
              Task.Factory.StartNew(
                  () =>
                  {
                      Mapper.DynamicMap<SomeDtoA, SomeDtoB>(new SomeDtoA());
                      Mapper.DynamicMap<SomeDtoB, SomeDtoA>(new SomeDtoB());
                      Mapper.DynamicMap<SomeDtoC, SomeDtoD>(new SomeDtoC());
                      Mapper.DynamicMap<SomeDtoD, SomeDtoC>(new SomeDtoD());
                  }))
          .ToArray();
            Exception exception = null;
            try
            {
                Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                exception = e;
            }
            exception.ShouldBeNull();
            //typeof(Exception).ShouldNotBeThrownBy(() => Task.WaitAll(tasks));
        }
        [Fact]
        public void Should_not_fail_with_create_map()
        {
            Mapper.Reset();

            var tasks = Enumerable.Range(0, 5).Select(
          i =>
              Task.Factory.StartNew(
                  () =>
                  {
                      Mapper.CreateMap<SomeDtoA, SomeDtoB>();
                      Mapper.CreateMap<SomeDtoB, SomeDtoA>();
                      Mapper.CreateMap<SomeDtoC, SomeDtoD>();
                      Mapper.CreateMap<SomeDtoD, SomeDtoC>();
                  }))
          .ToArray();
            Exception exception = null;
            try
            {
                Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                exception = e;
            }
            exception.ShouldBeNull();
            //typeof(Exception).ShouldNotBeThrownBy(() => Task.WaitAll(tasks));
        }
    }
}
#endif

// The three exceptions I saw while running the multithreading tests for DynamicMap (lbargaoanu)

//Unhandled Exception: System.AggregateException: One or more errors occurred. ---> AutoMapper.AutoMapperMappingException:

//Mapping types:
//SomeDtoB -> SomeDtoA
//TestConsole.Program+SomeDtoB -> TestConsole.Program+SomeDtoA

//Destination path:
//SomeDtoA

//Source value:
//TestConsole.Program+SomeDtoB ---> System.NullReferenceException: Object reference not set to an instance of an object.
//   at AutoMapper.MappingEngine.AutoMapper.IMappingEngineRunner.Map(ResolutionContext context) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.c
//s:line 256
//   --- End of inner exception stack trace ---
//   at AutoMapper.MappingEngine.AutoMapper.IMappingEngineRunner.Map(ResolutionContext context) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.c
//s:line 264
//   at AutoMapper.MappingEngine.DynamicMap(Object source, Type sourceType, Type destinationType) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine
//.cs:line 199
//   at AutoMapper.MappingEngine.DynamicMap[TSource, TDestination](TSource source) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.cs:line 170
//   at AutoMapper.Mapper.DynamicMap[TSource, TDestination](TSource source) in D:\Projects\AutoMapper\src\AutoMapper\Mapper.cs:line 174
//   at TestConsole.Program.<>c.<Main>b__6_1() in D:\Projects\TestConsole\TestConsole\Program.cs:line 141
//   at System.Threading.Tasks.Task.Execute()
//   --- End of inner exception stack trace ---
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks, Int32 millisecondsTimeout, CancellationToken cancellationToken)
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks, Int32 millisecondsTimeout)
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks)
//   at TestConsole.Program.Main(String[] args) in D:\Projects\TestConsole\TestConsole\Program.cs:line 146

//Unhandled Exception: System.AggregateException: One or more errors occurred. ---> AutoMapper.AutoMapperMappingException:

//Mapping types:
//SomeDtoB -> SomeDtoA
//TestConsole.Program+SomeDtoB -> TestConsole.Program+SomeDtoA

//Destination path:
//SomeDtoA

//Source value:
//TestConsole.Program+SomeDtoB ---> System.NullReferenceException: Object reference not set to an instance of an object.
//   at AutoMapper.Mappers.TypeMapMapper.Map(ResolutionContext context, IMappingEngineRunner mapper) in D:\Projects\AutoMapper\src\AutoMapper\Mappers\Ty
//peMapMapper.cs:line 17
//   at AutoMapper.MappingEngine.AutoMapper.IMappingEngineRunner.Map(ResolutionContext context) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.c
//s:line 260
//   --- End of inner exception stack trace ---
//   at AutoMapper.MappingEngine.AutoMapper.IMappingEngineRunner.Map(ResolutionContext context) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.c
//s:line 268
//   at AutoMapper.MappingEngine.DynamicMap(Object source, Type sourceType, Type destinationType) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine
//.cs:line 199
//   at AutoMapper.MappingEngine.DynamicMap[TSource, TDestination](TSource source) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.cs:line 170
//   at AutoMapper.Mapper.DynamicMap[TSource, TDestination](TSource source) in D:\Projects\AutoMapper\src\AutoMapper\Mapper.cs:line 174
//   at TestConsole.Program.<>c.<Main>b__6_1() in D:\Projects\TestConsole\TestConsole\Program.cs:line 141
//   at System.Threading.Tasks.Task.Execute()
//   --- End of inner exception stack trace ---
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks, Int32 millisecondsTimeout, CancellationToken cancellationToken)
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks, Int32 millisecondsTimeout)
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks)
//   at TestConsole.Program.Main(String[] args) in D:\Projects\TestConsole\TestConsole\Program.cs:line 145

//Unhandled Exception: System.AggregateException: One or more errors occurred. ---> System.Collections.Generic.KeyNotFoundException: The given key was n
//ot present in the dictionary.
//   at System.Collections.Concurrent.ConcurrentDictionary`2.get_Item(TKey key)
//   at AutoMapper.Internal.DictionaryFactoryOverride.ConcurrentDictionaryImpl`2.get_Item(TKey key) in D:\Projects\AutoMapper\src\AutoMapper\Internal\Co
//ncurrentDictionaryFactory.cs:line 42
//   at AutoMapper.ConfigurationStore.<ResolveTypeMap>b__87_1(TypePair tp) in D:\Projects\AutoMapper\src\AutoMapper\ConfigurationStore.cs:line 356
//   at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()
//   at System.Linq.Enumerable.FirstOrDefault[TSource](IEnumerable`1 source, Func`2 predicate)
//   at AutoMapper.ConfigurationStore.<ResolveTypeMap>b__87_0(TypePair _) in D:\Projects\AutoMapper\src\AutoMapper\ConfigurationStore.cs:line 353
//   at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
//   at AutoMapper.Internal.DictionaryFactoryOverride.ConcurrentDictionaryImpl`2.GetOrAdd(TKey key, Func`2 valueFactory) in D:\Projects\AutoMapper\src\A
//utoMapper\Internal\ConcurrentDictionaryFactory.cs:line 37
//   at AutoMapper.ConfigurationStore.ResolveTypeMap(TypePair typePair) in D:\Projects\AutoMapper\src\AutoMapper\ConfigurationStore.cs:line 351
//   at AutoMapper.ConfigurationStore.ResolveTypeMap(Type sourceType, Type destinationType) in D:\Projects\AutoMapper\src\AutoMapper\ConfigurationStore.
//cs:line 346
//   at AutoMapper.ConfigurationStore.ResolveTypeMap(Object source, Object destination, Type sourceType, Type destinationType) in D:\Projects\AutoMapper
//\src\AutoMapper\ConfigurationStore.cs:line 364
//   at AutoMapper.MappingEngine.DynamicMap(Object source, Type sourceType, Type destinationType) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine
//.cs:line 191
//   at AutoMapper.MappingEngine.DynamicMap[TSource, TDestination](TSource source) in D:\Projects\AutoMapper\src\AutoMapper\MappingEngine.cs:line 170
//   at AutoMapper.Mapper.DynamicMap[TSource, TDestination](TSource source) in D:\Projects\AutoMapper\src\AutoMapper\Mapper.cs:line 174
//   at TestConsole.Program.<>c.<Main>b__6_1() in D:\Projects\TestConsole\TestConsole\Program.cs:line 143
//   at System.Threading.Tasks.Task.Execute()
//   --- End of inner exception stack trace ---
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks, Int32 millisecondsTimeout, CancellationToken cancellationToken)
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks, Int32 millisecondsTimeout)
//   at System.Threading.Tasks.Task.WaitAll(Task[] tasks)
//   at TestConsole.Program.Main(String[] args) in D:\Projects\TestConsole\TestConsole\Program.cs:line 145
