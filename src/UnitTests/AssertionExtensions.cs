namespace AutoMapper.UnitTests
{
    using System;
    using System.Collections;
    using System.Linq;
    using Should;
    using Should.Core.Exceptions;

    public delegate void ThrowingAction();

    // ReSharper disable UseStringInterpolation
    public static class AssertionExtensions
    {
        public static void ShouldNotBeThrownBy(this Type exception, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (exception.IsInstanceOfType(ex))
                {
                    throw new AssertException(
                        string.Format("Expected no exception of type {0} to be thrown.", exception), ex);
                }
            }
        }

        public static void ShouldContain(this IEnumerable items, object item)
        {
            CollectionAssertExtensions.ShouldContain(items.Cast<object>(), item);
        }

        public static void ShouldBeThrownBy(this Type exceptionType, ThrowingAction action)
        {
            Exception exception = null;

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            exception.ShouldNotBeNull();
            exception.ShouldBeType(exceptionType);
        }

        public static void ShouldBeInstanceOf<TExpectedType>(this object actual)
        {
            actual.ShouldBeType<TExpectedType>();
        }

        public static void ShouldNotBeInstanceOf<TExpectedType>(this object actual)
        {
            actual.ShouldNotBeType<TExpectedType>();
        }
    }
}