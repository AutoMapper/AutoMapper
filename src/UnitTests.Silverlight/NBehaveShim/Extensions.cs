using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace NBehave.Spec.NUnit
{
    public static class Extensions
    {
        public static Exception GetException(this Action action)
        {
            Exception e = null;
            try
            {
                action();
            }
            catch (Exception exc)
            {
                e = exc;
            }
            return e;
        }

        public static void ShouldApproximatelyEqual<T>(this T actual, T expected, T delta)
        {
            if (typeof(T) != typeof(decimal) && typeof(T) != typeof(double) && typeof(T) != typeof(float))
                Assert.Fail("type (T) must be float, double or decimal");

            if (typeof(T) == typeof(decimal))
                Assert.AreEqual(Decimal.ToDouble(Convert.ToDecimal(expected)),
                                Decimal.ToDouble(Convert.ToDecimal(actual)),
                                Decimal.ToDouble(Convert.ToDecimal(delta)));
            if (typeof(T) == typeof(double))
                Assert.AreEqual(Convert.ToDouble(expected), Convert.ToDouble(actual), Convert.ToDouble(delta));
            if (typeof(T) == typeof(float))
                Assert.AreEqual(Convert.ToSingle(expected), Convert.ToSingle(actual), Convert.ToSingle(delta));
        }

        public static void ShouldBeAssignableFrom<TExpectedType>(this Object actual)
        {
            Assert.IsAssignableFrom(typeof(TExpectedType), actual);
        }

        public static void ShouldBeAssignableFrom(this object actual, Type expected)
        {
            Assert.That(actual, Is.AssignableFrom(expected));
        }

        public static void ShouldBeEmpty(this string value)
        {
            Assert.That(value, Is.Empty);
        }

        public static void ShouldBeEmpty(this IEnumerable collection)
        {
            Assert.That(collection, Is.Empty);
        }

        public static void ShouldBeEqualTo(this ICollection actual, ICollection expected)
        {
            CollectionAssert.AreEqual(expected, actual);
        }

        public static void ShouldBeEquivalentTo(this ICollection actual, ICollection expected)
        {
            CollectionAssert.AreEquivalent(expected, actual);
        }

        public static void ShouldBeFalse(this bool condition)
        {
            Assert.That(condition, Is.False);
        }

        public static void ShouldBeGreaterThan(this IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2));
        }

        public static void ShouldBeGreaterThanOrEqualTo(this IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2));
        }

        [Obsolete("Use ShouldBeInstanceOfType")]
        public static void ShouldBeInstanceOf<TExpectedType>(this object actual)
        {
            actual.ShouldBeInstanceOfType(typeof(TExpectedType));
        }

        public static void ShouldBeInstanceOfType<TExpectedType>(this object actual)
        {
            actual.ShouldBeInstanceOfType(typeof(TExpectedType));
        }

        public static void ShouldBeInstanceOfType(this object actual, Type expected)
        {
            Assert.That(actual, Is.InstanceOf(expected));
        }

        public static void ShouldBeLessThan(this IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2));
        }

        public static void ShouldBeLessThanOrEqualTo(this IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2));
        }

        public static void ShouldBeNaN(this double value)
        {
            Assert.That(value, Is.NaN);
        }

        public static void ShouldBeNull(this object value)
        {
            Assert.That(value, Is.Null);
        }

        public static void ShouldBeTheSameAs<T>(this T actual, T expected) where T : class
        {
            Assert.That(actual, Is.SameAs(expected));
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
            e.ShouldBeInstanceOfType(exceptionType);

        }

        public static void ShouldBeTrue(this bool condition)
        {
            Assert.That(condition, Is.True);
        }

        public static void ShouldContain(this ICollection actual, object expected)
        {
            Assert.Contains(expected, actual);
        }

        public static void ShouldContain(this string actual, string expected)
        {
            StringAssert.Contains(expected, actual);
        }

        public static void ShouldContain(this IEnumerable actual, object expected)
        {

            var lst = new List<object>();
            foreach (var o in actual)
            {
                lst.Add(o);
            }
            ShouldContain(lst, expected);
        }

        public static void ShouldEndWith(this string value, string substring)
        {
            StringAssert.EndsWith(substring, value);
        }

        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }

        public static void ShouldMatch(this string value, Regex pattern)
        {
            Assert.IsTrue(pattern.IsMatch(value), string.Format("string \"{0}\" does not match pattern {1}", value, pattern));
        }

        public static void ShouldMatch(this string actual, string regexPattern, RegexOptions regexOptions)
        {
            Regex r = new Regex(regexPattern, regexOptions);
            ShouldMatch(actual, r);
        }

        public static void ShouldNotBeAssignableFrom<TExpectedType>(this object actual)
        {
            actual.ShouldNotBeAssignableFrom(typeof(TExpectedType));
        }

        public static void ShouldNotBeAssignableFrom(this object actual, Type expected)
        {
            Assert.That(actual, Is.Not.AssignableFrom(expected));
        }

        public static void ShouldNotBeEmpty(this string value)
        {
            Assert.That(value, Is.Not.Empty);
        }

        public static void ShouldNotBeEmpty(this ICollection collection)
        {
            Assert.That(collection, Is.Not.Empty);
        }

        public static void ShouldNotBeEqualTo(this ICollection actual, ICollection expected)
        {
            CollectionAssert.AreNotEqual(expected, actual);
        }

        public static void ShouldNotBeEquivalentTo(this ICollection actual, ICollection expected)
        {
            CollectionAssert.AreNotEquivalent(expected, actual);
        }

        [Obsolete("Use ShouldNotBeInstanceOfType")]
        public static void ShouldNotBeInstanceOf<TExpectedType>(this object actual)
        {
            actual.ShouldNotBeInstanceOfType(typeof(TExpectedType));
        }

        public static void ShouldNotBeInstanceOfType<TExpectedType>(this object actual)
        {
            actual.ShouldNotBeInstanceOfType(typeof(TExpectedType));
        }

        public static void ShouldNotBeInstanceOfType(this object actual, Type expected)
        {
            Assert.That(actual, Is.Not.InstanceOf(expected));
        }

        public static void ShouldNotBeNull(this object value)
        {
            Assert.That(value, Is.Not.Null);
        }

        public static void ShouldNotBeTheSameAs(this object actual, object notExpected)
        {
            Assert.That(actual, Is.Not.SameAs(notExpected));
        }

        public static void ShouldNotContain(this IEnumerable list, object expected)
        {
            Assert.That(list, Is.Not.Contains(expected));
        }

        public static void ShouldNotContain(this string actual, string expected)
        {
            Assert.IsTrue(actual.IndexOf(expected) == -1, string.Format("{0} should not contain {1}", actual, expected));
        }

        public static void ShouldNotEqual<T>(this T actual, T notExpected)
        {
            Assert.That(actual, Is.Not.EqualTo(notExpected));
        }

        public static void ShouldNotMatch(this string value, Regex pattern)
        {
            Assert.IsTrue(pattern.IsMatch(value) == false, string.Format("string \"{0}\" should not match pattern {1}", value, pattern));
        }

        public static void ShouldStartWith(this string value, string substring)
        {
            StringAssert.StartsWith(substring, value);
        }

        public static IActionSpecification<T> ShouldThrow<T>(this T value, Type exception)
        {
            return new ActionSpecification<T>(value, e =>
            {
                e.ShouldNotBeNull();
                e.ShouldBeInstanceOfType(exception);
            });
        }

        public static Exception ShouldThrow<T>(this Action action)
        {
            bool failed = false;
            var ex = new Exception("");
            try
            {
                action();
                failed = true;
            }
            catch (Exception e)
            {
                e.ShouldBeInstanceOfType(typeof(T));
                ex = e;
            }
            if (failed)
                Assert.Fail(string.Format("Exception of type <{0}> expected but no exception occurred", typeof(T)));
            return ex;
        }

        public static void WithExceptionMessage(this Exception e, string exceptionMessage)
        {
            exceptionMessage.ShouldEqual(e.Message);
        }
    }
}
