using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
   public abstract class ObjectMapperBase : IBenchmarker
	{
		int IBenchmarker.Iterations
		{
			get
			{
				return 1000000;
			}
		}

		string IBenchmarker.Name
		{
			get
			{
				return Name;
			}
		}

		void IBenchmarker.Execute()
		{
			Map();
		}

		abstract public string Name { get; }

		public abstract void Map();

		public abstract void Initialize();
	}
}
