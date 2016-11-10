using System;
using System.Collections.Generic;
using System.Reflection;

namespace Should.Core.Exceptions
{
    /// <summary>
    /// The base assert exception class
    /// </summary>
    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = "Should.";

        readonly string stackTrace;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        public AssertException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        public AssertException(string userMessage)
            : base(userMessage)
        {
            this.UserMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        /// <param name="innerException">The inner exception</param>
        public AssertException(string userMessage, Exception innerException)
            : base(userMessage, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed</param>
        /// <param name="stackTrace">The stack trace to be displayed</param>
        protected AssertException(string userMessage, string stackTrace)
            : base(userMessage)
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

        /// <summary>
        /// Gets the user message
        /// </summary>
        public string UserMessage { get; protected set; }

        /// <summary>
        /// Filters the stack trace to remove all lines that occur within the testing framework.
        /// </summary>
        /// <param name="stackTrace">The original stack trace</param>
        /// <returns>The filtered stack trace</returns>
        protected static string FilterStackTrace(string stackTrace)
        {
            if (stackTrace == null)
                return null;

            List<string> results = new List<string>();

            foreach (string line in SplitLines(stackTrace))
            {
                string trimmedLine = line.TrimStart();
                if (!trimmedLine.StartsWith( "at " + FilterStackTraceAssemblyPrefix) )
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        // Our own custom String.Split because Silverlight/CoreCLR doesn't support the version we were using
        static IEnumerable<string> SplitLines(string input)
        {
            while (true)
            {
                int idx = input.IndexOf(Environment.NewLine);

                if (idx < 0)
                {
                    yield return input;
                    break;
                }

                yield return input.Substring(0, idx);
                input = input.Substring(idx + Environment.NewLine.Length);
            }
        }
    }
}