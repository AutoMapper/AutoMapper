using System;
using System.Runtime.Serialization;

namespace AutoMapper
{
	[Serializable]
	public class AutoMapperMappingException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public AutoMapperMappingException()
		{
		}

		public AutoMapperMappingException(string message) : base(message)
		{
		}

		public AutoMapperMappingException(string message, Exception inner) : base(message, inner)
		{
		}

		public AutoMapperMappingException(ResolutionContext context)
			: this("Unable to perform mapping, view the ResolutionContext for more details. " + context)
		{
			Context = context;
		}

		public AutoMapperMappingException(ResolutionContext context, Exception inner)
			: this("Unable to perform mapping, view the ResolutionContext for more details. " + context, inner)
		{
			Context = context;
		}

		protected AutoMapperMappingException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

		public AutoMapperMappingException(ResolutionContext context, string message)
			: this("Unable to perform mapping, view the ResolutionContext for more details. " + context + "\r\n" + message)
		{
		}

		public ResolutionContext Context { get; private set; }
	}
}
