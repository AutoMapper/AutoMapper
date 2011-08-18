
using System ;
using System.Collections.Generic;
using System.Diagnostics ;
using System.Linq;
using System.Threading ;
using System.Threading.Tasks ;
using AutoMapper.Mappers;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
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

		[Test]
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
}
