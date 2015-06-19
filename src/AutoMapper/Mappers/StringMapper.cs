namespace AutoMapper.Mappers
{
    /// <summary>
    /// 
    /// </summary>
    public class StringMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            return context.SourceValue?.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            // So we know that == and != Type equality works...
            return context.DestinationType == typeof (string)
                   && context.SourceType != typeof (string);
        }
    }
}