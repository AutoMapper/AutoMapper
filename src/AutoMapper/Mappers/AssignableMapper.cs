namespace AutoMapper.Mappers
{
    /// <summary>
    /// 
    /// </summary>
    public class AssignableMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;
            if (context.SourceValue == null && !runner.ShouldMapSourceValueAsNull(context))
            {
                return runner.CreateObject(context);
            }

            return context.SourceValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return context.DestinationType.IsAssignableFrom(context.SourceType);
        }
    }
}