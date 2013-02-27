using System;

namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when code unexpectedly fails to throw an exception.
    /// </summary>
    public class ThrowsException : AssertActualExpectedException
    {
        readonly string stackTrace = null;

        /// <summary>
        /// Creates a new instance of the <see cref="ThrowsException"/> class. Call this constructor
        /// when no exception was thrown.
        /// </summary>
        /// <param name="expectedType">The type of the exception that was expected</param>
        public ThrowsException(Type expectedType)
            : this(expectedType, "(No exception was thrown)", null, null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ThrowsException"/> class. Call this constructor
        /// when an exception of the wrong type was thrown.
        /// </summary>
        /// <param name="expectedType">The type of the exception that was expected</param>
        /// <param name="actual">The actual exception that was thrown</param>
        public ThrowsException(Type expectedType,
                               Exception actual)
            : this(expectedType, actual.GetType().FullName, actual.Message, actual.StackTrace) { }

        ThrowsException(Type expected,
                        string actual,
                        string actualMessage,
                        string stackTrace)
            : base(expected,
                   actual + (actualMessage == null ? "" : ": " + actualMessage),
                   "Assert.Throws() Failure")
        {
            this.stackTrace = stackTrace;
        }

        /// <summary>
        /// Gets a string representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        /// <returns>A string that describes the contents of the call stack, with the most recent method call appearing first.</returns>
        public override string StackTrace
        {
            get { return FilterStackTrace(stackTrace ?? base.StackTrace); }
        }
    }
}