using System;

namespace Should.Core.Exceptions
{
    public abstract class ComparisonException : AssertException
    {
        public string Left { get; private set; }
        public string Right { get; private set; }

        protected ComparisonException(object left, object right, string methodName, string operation)
            : base(string.Format("Assert.{0}() Failure:\r\n\tExpected: {1} {2} {3}\r\n\tbut it was not", methodName, Format(right), operation, Format(left)))
        {
            Left = left != null ? left.ToString() : null;
            Right = right != null ? right.ToString() : null;
        }

        protected ComparisonException(object left, object right, string message) : base(message)
        {
            Left = left != null ? left.ToString() : null;
            Right = right != null ? right.ToString() : null;
        }

        public static string Format(object value)
        {
            if (value == null)
            {
                return "(null)";
            }
            var type = value.GetType();
            return type == typeof(string) // || type == typeof(DateTime) || type == typeof(DateTime?)
                ? string.Format("\"{0}\"", value)
                : value.ToString();
        }
    }
}