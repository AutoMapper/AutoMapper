using System;
using System.Collections.Generic;

namespace AutoMapper.Configuration
{
    public enum EnumMappingType
    {
        Value = 0,
        Name = 1
    }

    public class EnumConfigurationExpression<TSource, TDestination> : IEnumConfigurationExpression<TSource, TDestination>, IEnumMapConfiguration
    {
        protected EnumMappingType EnumMappingType = EnumMappingType.Value;
        protected readonly Dictionary<TSource, TDestination> EnumValueMappingsOverride = new Dictionary<TSource, TDestination>();

        public void Configure(TypeMap typeMap)
        {
            if (!typeMap.SourceType.IsEnum)
            {
                throw new ArgumentException($"The type {typeMap.SourceType.FullName} can not be configured as an Enum, because it is not an Enum");
            }

            if (!typeMap.DestinationTypeToUse.IsEnum)
            {
                throw new ArgumentException($"The type {typeMap.DestinationTypeToUse.FullName} can not be configured as an Enum, because it is not an Enum");
            }

            var enumValueMappings = new Dictionary<Enum, Enum>();

            var destinationEnumValues = Enum.GetValues(typeMap.DestinationTypeToUse);

            if (EnumMappingType == EnumMappingType.Name)
            {
                const bool ignoreCase = false;

                foreach (Enum destinationEnumValue in destinationEnumValues)
                {
                    var destinationEnumName = Enum.GetName(typeMap.DestinationTypeToUse, destinationEnumValue);

                    try
                    {
                        Enum sourceEnumValue = (Enum) Enum.Parse(typeMap.SourceType, destinationEnumName, ignoreCase);
                        enumValueMappings.Add(sourceEnumValue, destinationEnumValue);
                    }
                    catch
                    {
                        // missing type map, AssertConfigurationIsValid will throw exception
                    }
                }
            }
            else
            {
                var sourceEnumValueType = Enum.GetUnderlyingType(typeMap.SourceType);
                var destinationEnumValueType = Enum.GetUnderlyingType(typeMap.DestinationTypeToUse);

                foreach (Enum destinationEnumValue in destinationEnumValues)
                {
                    var sourceEnumValues = Enum.GetValues(typeMap.SourceType);
                    foreach (Enum sourceEnumValue in sourceEnumValues)
                    {
                        var compareSource = Convert.ChangeType(sourceEnumValue, sourceEnumValueType);
                        var compareDestination = Convert.ChangeType(destinationEnumValue, destinationEnumValueType);

                        if (compareSource.Equals(compareDestination))
                        {
                            enumValueMappings.Add(sourceEnumValue, destinationEnumValue);
                        }
                    }
                }
            }

            foreach (var enumValueMappingOverride in EnumValueMappingsOverride)
            {
                enumValueMappings[enumValueMappingOverride.Key as Enum] = enumValueMappingOverride.Value as Enum;
            }

            typeMap.ConfigureEnumMappings(EnumMappingType, enumValueMappings);
        }

        public IEnumMapConfiguration Reverse()
        {
            var reverseEnumConfigurationExpression = new EnumConfigurationExpression<TDestination, TSource>();

            // TODO: what to do with destinations with multiple sources?
            foreach (var enumValueMappingOverride in EnumValueMappingsOverride)
            {
                reverseEnumConfigurationExpression.MapValue(enumValueMappingOverride.Value, enumValueMappingOverride.Key);
            }

            return reverseEnumConfigurationExpression;
        }

        public IEnumConfigurationExpression<TSource, TDestination> MapByName()
        {
            EnumMappingType = EnumMappingType.Name;
            return this;
        }

        public IEnumConfigurationExpression<TSource, TDestination> MapByValue()
        {
            EnumMappingType = EnumMappingType.Value;
            return this;
        }

        public IEnumConfigurationExpression<TSource, TDestination> MapValue(TSource source, TDestination destination)
        {
            EnumValueMappingsOverride[source] = destination;
            return this;
        }
    }
}