using System;
using System.Runtime.Serialization;

namespace AutoMapper
{
	[Serializable]
	public class AutoMapperMappingException : Exception
	{
		private string _message;

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
			_message = message;
		}

		public AutoMapperMappingException(string message, Exception inner) : base(null, inner)
		{
			_message = message;
		}

		public AutoMapperMappingException(ResolutionContext context)
		{
			Context = context;
		}

		public AutoMapperMappingException(ResolutionContext context, Exception inner)
			: base(null, inner)
		{
			Context = context;
		}

		protected AutoMapperMappingException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

		public AutoMapperMappingException(ResolutionContext context, string message)
			: this(context)
		{
			_message = message;
		}

		public ResolutionContext Context { get; private set; }

		public override string Message
		{
			get
			{
				string message = null;
				if (Context != null)
				{
					message = string.Format("Trying to map {0} to {1}.", Context.SourceType.Name, Context.DestinationType.Name);
					TypeMap contextTypeMap = Context.GetContextTypeMap();
					if (contextTypeMap != null)
					{
						message += string.Format("\nUsing mapping configuration for {0} to {1}", contextTypeMap.SourceType, contextTypeMap.DestinationType);
					}
					if (Context.TypeMap != null && Context.TypeMap != contextTypeMap)
					{
						message += string.Format("\nUsing property mapping configuration for {0} to {1}", Context.TypeMap.SourceType, Context.TypeMap.DestinationType);
					}
					if (Context.PropertyMap != null)
					{
						message += string.Format("\nDestination property: {0}", Context.PropertyMap.DestinationProperty.Name);
					}
				}
				if (_message != null)
				{
					message = (message == null ? null : message + "\n") + _message;
				}
				if (base.Message != null)
				{
					message = (message == null ? null : message + "\n") + base.Message;
				}
				return message;
			}
		}
	}
}
