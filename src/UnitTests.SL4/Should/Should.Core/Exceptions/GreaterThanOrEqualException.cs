namespace Should.Core.Exceptions
{
    /// <summary>Exception thrown when a value is not greater than the expected minimum.</summary>
    public class GreaterThanOrEqualException : ComparisonException
    {
        /// <summary>Initializes a new instance of the <see cref="GreaterThanOrEqualException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive minimum allowed value.</param>
        public GreaterThanOrEqualException(object left, object right) 
            : base(right, left, "GreaterThanOrEqual", ">=")
        { }

        /// <summary>Initializes a new instance of the <see cref="GreaterThanOrEqualException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive minimum allowed value.</param>
        public GreaterThanOrEqualException(object left, object right, string message)
            : base(left, right, message) 
        { }
    }
}