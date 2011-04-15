using System;

namespace AutoMapper
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class IgnoreMapAttribute : Attribute
	{
	}
}
