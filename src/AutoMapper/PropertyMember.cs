using System;
using System.Reflection;

namespace AutoMapper
{
	internal abstract class TypeMember : IValueResolver
	{
		public abstract Type GetResolvedValueType();
		public abstract ResolutionResult Resolve(ResolutionResult source);
	}

	internal class PropertyMember : TypeMember
	{
		private readonly PropertyInfo _property;

		public PropertyMember(PropertyInfo property)
		{
			_property = property;
		}

		public override Type GetResolvedValueType()
		{
			return _property.PropertyType;
		}

		public override ResolutionResult Resolve(ResolutionResult source)
		{
			return source.Value == null
				? new ResolutionResult(source.Value, _property.PropertyType)
				: new ResolutionResult(_property.GetValue(source.Value, new object[0]), _property.PropertyType);
		}
	}
}