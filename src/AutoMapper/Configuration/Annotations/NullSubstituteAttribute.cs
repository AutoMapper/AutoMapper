using System;

namespace AutoMapper.Configuration.Annotations
{
    public sealed class NullSubstituteAttribute : Attribute, IMemberConfigurationProvider
    {
        /// <summary>
        /// Value to use if source value is null
        /// </summary>
        public object Value { get; }

        public NullSubstituteAttribute(object value)
        {
            Value = value;
        }

        public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
        {
            memberConfigurationExpression.NullSubstitute(Value);
        }
    }
}