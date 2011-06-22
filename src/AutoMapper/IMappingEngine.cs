using System;

namespace AutoMapper
{
    public interface IMappingEngine
    {
        TDestination Map<TDestination>(object source);
        TDestination Map<TSource, TDestination>(TSource source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        object Map(object source, Type sourceType, Type destinationType);
        object Map(object source, object destination, Type sourceType, Type destinationType);
    	TDestination DynamicMap<TSource, TDestination>(TSource source);
    	TDestination DynamicMap<TDestination>(object source);
    	object DynamicMap(object source, Type sourceType, Type destinationType);
		void DynamicMap<TSource, TDestination>(TSource source, TDestination destination);
    	void DynamicMap(object source, object destination, Type sourceType, Type destinationType);
    }
}

