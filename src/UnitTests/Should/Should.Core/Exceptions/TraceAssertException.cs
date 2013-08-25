using System;

namespace Should.Core.Exceptions
{
    /// <summary>
    /// Exception that is thrown when a call to Debug.Assert() fails.
    /// </summary>
    public class TraceAssertException : AssertException
    {
        readonly string assertDetailedMessage;
        readonly string assertMessage;

        /// <summary>
        /// Creates a new instance of the <see cref="TraceAssertException"/> class.
        /// </summary>
        /// <param name="assertMessage">The original assert message</param>
        public TraceAssertException(string assertMessage)
            : this(assertMessage, "") { }

        /// <summary>
        /// Creates a new instance of the <see cref="TraceAssertException"/> class.
        /// </summary>
        /// <param name="assertMessage">The original assert message</param>
        /// <param name="assertDetailedMessage">The original assert detailed message</param>
        public TraceAssertException(string assertMessage,
                                    string assertDetailedMessage)
        {
            this.assertMessage = assertMessage ?? "";
            this.assertDetailedMessage = assertDetailedMessage ?? "";
        }

        /// <summary>
        /// Gets the original assert detailed message.
        /// </summary>
        public string AssertDetailedMessage
        {
            get { return assertDetailedMessage; }
        }

        /// <summary>
        /// Gets the original assert message.
        /// </summary>
        public string AssertMessage
        {
            get { return assertMessage; }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                string result = "Debug.Assert() Failure";

                if (AssertMessage != "")
                {
                    result += " : " + AssertMessage;

                    if (AssertDetailedMessage != "")
                        result += Environment.NewLine + "Detailed Message:" + Environment.NewLine + AssertDetailedMessage;
                }

                return result;
            }
        }
    }
}