namespace Should.Core.Exceptions
{
    /// <summary>Exception thrown when a value is not greater than the expected minimum.</summary>
    public class GreaterThanException : ComparisonException
    {
        /// <summary>Initializes a new instance of the <see cref="GreaterThanException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive minimum allowed value.</param>
        public GreaterThanException(object left, object right) 
            : base(right, left, "GreaterThan", ">")
        { }

        /// <summary>Initializes a new instance of the <see cref="GreaterThanException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive minimum allowed value.</param>
        public GreaterThanException(object left, object right, string message)
            : base(left, right, message)
        { }
    }
}