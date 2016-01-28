namespace AutoMapper.Mappers
{
    using System;
    using System.Linq;
    using Internal;

    public class FlagsEnumMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            Type enumDestType = TypeHelper.GetEnumerationType(context.DestinationType);

            if (context.SourceValue == null)
            {
                return context.Engine.CreateObject(context);
            }

            return Enum.Parse(enumDestType, context.SourceValue.ToString(), true);
        }

        public bool IsMatch(TypePair context)
        {
            var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
            var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

            return sourceEnumType != null
                   && destEnumType != null
                   && sourceEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any()
                   && destEnumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any();
        }
    }
}