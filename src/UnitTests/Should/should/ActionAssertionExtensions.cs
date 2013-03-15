using System;
using Should.Core.Assertions;

namespace Should
{
    public static class ActionAssertionExtensions
    {
        /// <summary>Verifies that the <paramref name="action"/> throws the specified exception type.</summary>
        /// <typeparam name="T">The type of exception expected to be thrown.</typeparam>
        /// <param name="action">The action which should throw the exception.</param>
        public static void ShouldThrow<T>(this Action action) where T : Exception
        {
            ShouldThrow<T>(new Assert.ThrowsDelegate(action));
        }

        /// <summary>Verifies that the <paramref name="@delegate"/> throws the specified exception type.</summary>
        /// <typeparam name="T">The type of exception expected to be thrown.</typeparam>
        /// <param name="delegate">A <see cref="Assert.ThrowsDelegate"/> which represents the action which should throw the exception.</param>
        public static void ShouldThrow<T>(this Assert.ThrowsDelegate @delegate) where T : Exception
        {
            Assert.Throws<T>(@delegate);
        }
    }
}