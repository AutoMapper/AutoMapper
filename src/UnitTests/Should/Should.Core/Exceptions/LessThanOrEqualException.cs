namespace Should.Core.Exceptions
{
    /// <summary>Exception thrown when a value is not less than or equal to the expected maximum.</summary>
    public class LessThanOrEqualException : ComparisonException
    {
        /// <summary>Initializes a new instance of the <see cref="LessThanOrEqualException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive maximum allowed value.</param>
        public LessThanOrEqualException(object left, object right) 
            : base(right, left, "LessThanOrEqual", "<=")
        { }

        /// <summary>Initializes a new instance of the <see cref="LessThanOrEqualException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive maximum allowed value.</param>
        public LessThanOrEqualException(object left, object right, string message)
            : base(left, right, message)
        { }
    }
}