using System;

namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when the value is unexpectedly not of the given type or a derived type.
    /// </summary>
    public class IsAssignableFromException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IsTypeException"/> class.
        /// </summary>
        /// <param name="expected">The expected type</param>
        /// <param name="actual">The actual object value</param>
        public IsAssignableFromException(Type expected, object actual) : this(expected, actual, "Assert.IsAssignableFrom() Failure") { }

        /// <summary>
        /// Creates a new instance of the <see cref="IsTypeException"/> class.
        /// </summary>
        /// <param name="expected">The expected type</param>
        /// <param name="actual">The actual object value</param>
        /// <param name="userMessage">A custom message to prepend to the default Assert.IsAssignableFrom() failure message</param>
        public IsAssignableFromException(Type expected, object actual, string userMessage)
            : base(expected, actual == null ? null : actual.GetType(), userMessage) { }
    }
}