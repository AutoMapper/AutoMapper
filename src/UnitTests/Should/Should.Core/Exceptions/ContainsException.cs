namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly does not contain the expected value.
    /// </summary>
    public class ContainsException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ContainsException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        public ContainsException(object expected)
            : base(string.Format("Assert.Contains() failure: Not found: {0}", expected)) { }
    }
}