using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeMapMapper : IObjectMapper
	{
        /// <summary>
        /// 
        /// </summary>
        private readonly IEnumerable<ITypeMapObjectMapper> _mappers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappers"></param>
	    public TypeMapMapper(IEnumerable<ITypeMapObjectMapper> mappers)
        {
            _mappers = mappers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
	    public object Map(ResolutionContext context)
		{
	        context.TypeMap.Seal();

            var mapperToUse = _mappers.First(objectMapper => objectMapper.IsMatch(context));

	        // check whether the context passes conditions before attempting to map the value (depth check)
            var mappedObject = !context.TypeMap.ShouldAssignValue(context)
                ? null
                : mapperToUse.Map(context);

	        return mappedObject;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
		public bool IsMatch(ResolutionContext context)
		{
			return context.TypeMap != null;
		}
	}
}