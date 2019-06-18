﻿using System;

namespace AutoMapper.Configuration.Annotations
{
    /// <summary>
    /// Substitute a custom value when the source member resolves as null
    /// </summary>
    /// <remarks>
    /// Must be used in combination with <see cref="AutoMapAttribute" />
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
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