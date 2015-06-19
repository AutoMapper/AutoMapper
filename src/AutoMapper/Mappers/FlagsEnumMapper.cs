namespace AutoMapper.Mappers
{
    using System;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public class FlagsEnumMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;
            var enumDestType = context.DestinationType.GetEnumerationType();

            if (context.SourceValue == null)
            {
                return runner.CreateObject(context);
            }

            return Enum.Parse(enumDestType, context.SourceValue.ToString(), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            var sourceEnumType = context.SourceType.GetEnumerationType();
            var destEnumType = context.DestinationType.GetEnumerationType();

            return !(sourceEnumType == null || destEnumType == null)
                   && sourceEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any()
                   && destEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any();
        }
    }
}