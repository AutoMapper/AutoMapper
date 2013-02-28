namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly does not contain the expected value.
    /// </summary>
    public class StartsWithException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ContainsException"></see> class.
        /// </summary>
        /// <param name="expectedStartString">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        public StartsWithException(object expectedStartString, object actual)
            : base(string.Format("Assert.StartsWith() failure: '{0}' not found at the beginning of '{1}'", expectedStartString, actual)) { }
    }
}
