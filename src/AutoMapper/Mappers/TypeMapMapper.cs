using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
    public class TypeMapMapper : IObjectMapper
	{
        private readonly IEnumerable<ITypeMapObjectMapper> _mappers;

	    public TypeMapMapper(IEnumerable<ITypeMapObjectMapper> mappers)
        {
            _mappers = mappers;
        }

	    public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
	        context.TypeMap.Seal();

	        var mapperToUse = _mappers.First(objectMapper => objectMapper.IsMatch(context, mapper));
            object mappedObject = mapperToUse.Map(context, mapper);

            context.TypeMap.AfterMap(context.SourceValue, mappedObject);
	        return mappedObject;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.TypeMap != null;
		}
	}
}