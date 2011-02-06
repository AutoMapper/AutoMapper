using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoMapper
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class IgnoreMapAttribute : Attribute
	{
	}
}
