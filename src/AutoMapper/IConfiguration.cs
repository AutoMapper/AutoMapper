using System;

namespace AutoMapper
{
	public interface IConfiguration
	{
		TypeMap[] GetAllTypeMaps();
		TypeMap FindTypeMapFor(Type sourceType, Type destinationType);
		TypeMap FindTypeMapFor<TSource, TDestination>();
		IValueFormatter GetValueFormatter();
		IValueFormatter GetValueFormatter(string profileName);
		void AssertConfigurationIsValid();
	}

}