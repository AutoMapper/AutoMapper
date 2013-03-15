namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when two object references are unexpectedly not the same instance.
    /// </summary>
    public class SameException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SameException"/> class.
        /// </summary>
        /// <param name="expected">The expected object reference</param>
        /// <param name="actual">The actual object reference</param>
        public SameException(object expected,
                             object actual)
            : base(expected, actual, "Assert.Same() Failure", true) { }
    }
}