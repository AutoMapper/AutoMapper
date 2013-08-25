namespace Should.Core.Exceptions
{
    /// <summary>Exception thrown when a value is not less than the expected maximum.</summary>
    public class LessThanException : ComparisonException
    {
        /// <summary>Initializes a new instance of the <see cref="LessThanException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive maximum allowed value.</param>
        public LessThanException(object left, object right) 
            : base(right, left, "LessThan", "<")
        { }

        /// <summary>Initializes a new instance of the <see cref="LessThanException"/> class.</summary>
        /// <param name="left">The value being tested.</param>
        /// <param name="right">The exclusive maximum allowed value.</param>
        public LessThanException(object left, object right, string message)
            : base(left, right, message)
        { }
    }
}