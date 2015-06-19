namespace AutoMapper.Mappers
{
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class NullableSourceMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            return context.SourceValue
                ?? context.MapperContext.Runner.CreateObject(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return context.SourceType.IsNullableType() && !context.DestinationType.IsNullableType();
        }
    }
}