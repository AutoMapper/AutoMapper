namespace AutoMapper.Mappers
{
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class OpenGenericMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var typeMap = context.MapperContext.ConfigurationProvider.FindClosedGenericTypeMapFor(context);

            var newContext = context.CreateTypeContext(typeMap, context.SourceValue, context.DestinationValue, context.SourceType,
                context.DestinationType);

            return context.MapperContext.Runner.Map(newContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return context.TypeMap == null
                   && context.SourceType.IsGenericType()
                   && context.DestinationType.IsGenericType()
                   && !(context.SourceType.GetGenericTypeDefinition() == null
                        || context.DestinationType.GetGenericTypeDefinition() == null)
                   && context.MapperContext.ConfigurationProvider.HasOpenGenericTypeMapDefined(context);
        }
    }
}