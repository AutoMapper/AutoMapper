using System;
using System.Collections;
using System.Collections.Generic;
using Should.Core.Assertions;

namespace Should.Core.Exceptions
{
    /// <summary>
    /// Base class for exceptions that have actual and expected values
    /// </summary>
    public class AssertActualExpectedException : AssertException
    {
        readonly string actual;
        readonly string differencePosition = "";
        readonly string expected;

        /// <summary>
        /// Creates a new instance of the <see href="AssertActualExpectedException"/> class.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="userMessage">The user message to be shown</param>
        public AssertActualExpectedException(object expected,
                                             object actual,
                                             string userMessage)
            : this(expected, actual, userMessage, false) { }

        /// <summary>
        /// Creates a new instance of the <see href="AssertActualExpectedException"/> class.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="userMessage">The user message to be shown</param>
        /// <param name="skipPositionCheck">Set to true to skip the check for difference position</param>
        public AssertActualExpectedException(object expected,
                                             object actual,
                                             string userMessage,
                                             bool skipPositionCheck)
            : base(userMessage)
        {
            if (!skipPositionCheck)
            {
                IEnumerable enumerableActual = actual as IEnumerable;
                IEnumerable enumerableExpected = expected as IEnumerable;

                if (enumerableActual != null && enumerableExpected != null)
                {
                    var comparer = new EnumerableEqualityComparer();
                    comparer.Equals(enumerableActual, enumerableExpected);

                    differencePosition = "Position: First difference is at position " + comparer.Position + Environment.NewLine;
                }
            }

            this.actual = actual == null ? null : ConvertToString(actual);
            this.expected = expected == null ? null : ConvertToString(expected);

            if (actual != null &&
                expected != null &&
                actual.ToString() == expected.ToString() &&
                actual.GetType() != expected.GetType())
            {
                this.actual += String.Format(" ({0})", actual.GetType().FullName);
                this.expected += String.Format(" ({0})", expected.GetType().FullName);
            }
        }

        /// <summary>
        /// Gets the actual value.
        /// </summary>
        public string Actual
        {
            get { return actual; }
        }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        public string Expected
        {
            get { return expected; }
        }

        /// <summary>
        /// Gets a message that describes the current exception. Includes the expected and actual values.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        /// <filterpriority>1</filterpriority>
        public override string Message
        {
            get
            {
                return string.Format("{0}{4}{1}Expected: {2}{4}Actual:   {3}",
                                     base.Message,
                                     differencePosition,
                                     FormatMultiLine(Expected ?? "(null)"),
                                     FormatMultiLine(Actual ?? "(null)"),
                                     Environment.NewLine);
            }
        }

        static string ConvertToString(object value)
        {
            Array valueArray = value as Array;
            if (valueArray == null)
                return value.ToString();

            List<string> valueStrings = new List<string>();

            foreach (object valueObject in valueArray)
                valueStrings.Add(valueObject == null ? "(null)" : valueObject.ToString());

            return value.GetType().FullName + " { " + String.Join(", ", valueStrings.ToArray()) + " }";
        }

        static string FormatMultiLine(string value)
        {
            return value.Replace(Environment.NewLine, Environment.NewLine + "          ");
        }
    }
}