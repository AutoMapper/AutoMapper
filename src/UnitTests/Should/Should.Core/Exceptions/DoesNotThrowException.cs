using System;

namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when code unexpectedly throws an exception.
    /// </summary>
    public class DoesNotThrowException : AssertActualExpectedException
    {
        readonly string stackTrace;

        /// <summary>
        /// Creates a new instance of the <see cref="DoesNotThrowException"/> class.
        /// </summary>
        /// <param name="actual">Actual exception</param>
        public DoesNotThrowException(Exception actual)
            : base("(No exception)",
                   actual.GetType().FullName + (actual.Message == null ? "" : ": " + actual.Message),
                   "Assert.DoesNotThrow() failure",
                   true)
        {
            stackTrace = actual.StackTrace;
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