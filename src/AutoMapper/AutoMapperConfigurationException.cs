using System;
using System.Runtime.Serialization;

namespace AutoMapper
{
	[Serializable]
	public class AutoMapperConfigurationException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public AutoMapperConfigurationException()
		{
		}

		public AutoMapperConfigurationException(string message) : base(message)
		{
		}

		public AutoMapperConfigurationException(string message, Exception inner) : base(message, inner)
		{
		}

		public AutoMapperConfigurationException(TypeMap typeMap, string[] unmappedPropertyNames)
			: base(string.Format(
					"The following {3} properties on {0} are not mapped: \n\t{2}\nAdd a custom mapping expression, ignore, or rename the property on {1}.",
					typeMap.DestinationType.Name, typeMap.SourceType.Name, string.Join("\n\t", unmappedPropertyNames),
					unmappedPropertyNames.Length))
		{
		}

		public AutoMapperConfigurationException(TypeMap typeMap, string mismatchedPropertyName)
			: base(string.Format(
					"The following property on {0} is cannot be mapped: \n\t{2}\nAdd a custom mapping expression, ignore, add a custom resolver, or modify the destination type {1}.",
					typeMap.DestinationType.Name, typeMap.SourceType.Name, mismatchedPropertyName))
		{
		}

		protected AutoMapperConfigurationException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}