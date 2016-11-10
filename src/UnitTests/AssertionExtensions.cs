using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Should.Core.Exceptions;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    public delegate void ThrowingAction();

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
                    throw new AssertException(string.Format("Expected no exception of type {0} to be thrown.", exception), ex);
                }
            }
        }

        public static void ShouldContain(this IEnumerable items, object item)
        {
            CollectionAssertExtensions.ShouldContain(items.Cast<object>(), item);
        }

        public static void ShouldBeThrownBy(this Type exceptionType, ThrowingAction action)
        {
            Exception e = null;

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.ShouldNotBeNull();
            e.ShouldBeType(exceptionType);
        }

        public static void ShouldNotBeInstanceOf<TExpectedType>(this object actual)
        {
            actual.ShouldNotBeType<TExpectedType>();
        }
    }
}