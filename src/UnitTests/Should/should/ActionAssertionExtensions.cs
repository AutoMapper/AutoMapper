using System;
using Should.Core.Assertions;

namespace Should
{
    public static class ActionAssertionExtensions
    {
        /// <summary>Verifies that the <paramref name="action"/> throws the specified exception type.</summary>
        /// <typeparam name="T">The type of exception expected to be thrown.</typeparam>
        /// <param name="action">The action which should throw the exception.</param>
        /// <param name="exceptionChecker">Additional checks on the exception object.</param>
        public static void ShouldThrow<T>(this Action action, Action<T> exceptionChecker = null) where T : Exception
        {
            ShouldThrow<T>(new Assert.ThrowsDelegate(action), exceptionChecker);
        }

        /// <summary>Verifies that the <paramref name="@delegate"/> throws the specified exception type.</summary>
        /// <typeparam name="T">The type of exception expected to be thrown.</typeparam>
        /// <param name="delegate">A <see cref="Assert.ThrowsDelegate"/> which represents the action which should throw the exception.</param>
        /// <param name="exceptionChecker">Additional checks on the exception object.</param>
        public static void ShouldThrow<T>(this Assert.ThrowsDelegate @delegate, Action<T> exceptionChecker = null) where T : Exception
        {
            var exception = Assert.Throws<T>(@delegate);
            exceptionChecker?.Invoke(exception);
        }
    }
}