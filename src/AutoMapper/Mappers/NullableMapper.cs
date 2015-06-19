namespace AutoMapper.Mappers
{
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class NullableMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            return context.SourceValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return context.DestinationType.IsNullableType();
        }
    }
}