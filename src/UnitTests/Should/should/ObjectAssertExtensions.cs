using System;
using System.Collections.Generic;
using Should.Core.Assertions;
using Should.Core.Exceptions;

namespace Should
{
    /// <summary>
    /// Extensions which provide assertions to classes derived from <see cref="object"/>.
    /// </summary>
    public static class ObjectAssertExtensions
    {
        /// <summary>Verifies that an object is greater than the exclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (exclusive) minimum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        public static void ShouldBeGreaterThan<T>(this T @object, T value)
        {
            Assert.GreaterThan(@object, value);
        }

        /// <summary>Verifies that an object is greater than the exclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (exclusive) minimum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void ShouldBeGreaterThan<T>(this T @object, T value, IComparer<T> comparer)
        {
            Assert.GreaterThan(@object, value, comparer);
        }

        /// <summary>Verifies that an object is greater than the inclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (inclusive) minimum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        public static void ShouldBeGreaterThanOrEqualTo<T>(this T @object, T value)
        {
            Assert.GreaterThanOrEqual(@object, value);
        }

        /// <summary>Verifies that an object is greater than the inclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (inclusive) minimum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void ShouldBeGreaterThanOrEqualTo<T>(this T @object, T value, IComparer<T> comparer)
        {
            Assert.GreaterThanOrEqual(@object, value, comparer);
        }

        /// <summary>
        /// Verifies that a value is within a given range.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <exception cref="InRangeException">Thrown when the value is not in the given range</exception>
        public static void ShouldBeInRange<T>(this T actual,
                                              T low,
                                              T high)
        {
            Assert.InRange(actual, low, high);
        }

        /// <summary>
        /// Verifies that a value is within a given range, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <param name="comparer">The comparer used to evaluate the value's range</param>
        /// <exception cref="InRangeException">Thrown when the value is not in the given range</exception>
        public static void ShouldBeInRange<T>(this T actual,
                                              T low,
                                              T high,
                                              IComparer<T> comparer)
        {
            Assert.InRange(actual, low, high, comparer);
        }

        /// <summary>Verifies that an object is less than the exclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (exclusive) maximum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        public static void ShouldBeLessThan<T>(this T @object, T value)
        {
            Assert.LessThan(@object, value);
        }

        /// <summary>Verifies that an object is less than the exclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (exclusive) maximum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void ShouldBeLessThan<T>(this T @object, T value, IComparer<T> comparer)
        {
            Assert.LessThan(@object, value, comparer);
        }

        /// <summary>Verifies that an object is less than the inclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (inclusive) maximum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        public static void ShouldBeLessThanOrEqualTo<T>(this T @object, T value)
        {
            Assert.LessThanOrEqual(@object, value);
        }

        /// <summary>Verifies that an object is less than the inclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="object">The object to be evaluated (left side of the comparison).</param>
        /// <param name="value">The (inclusive) maximum value of the <paramref name="object"/>. (The right side of the comparison.)</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void ShouldBeLessThanOrEqualTo<T>(this T @object, T value, IComparer<T> comparer)
        {
            Assert.LessThanOrEqual(@object, value, comparer);
        }

        /// <summary>
        /// Verifies that an object reference is null.
        /// </summary>
        /// <param name="object">The object to be inspected</param>
        /// <exception cref="NullException">Thrown when the object reference is not null</exception>
        public static void ShouldBeNull(this object @object)
        {
            Assert.Null(@object);
        }

        /// <summary>
        /// Verifies that two objects are the same instance.
        /// </summary>
        /// <param name="actual">The actual object instance</param>
        /// <param name="expected">The expected object instance</param>
        /// <exception cref="SameException">Thrown when the objects are not the same instance</exception>
        public static void ShouldBeSameAs(this object actual,
                                          object expected)
        {
            Assert.Same(expected, actual);
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T ShouldBeType<T>(this object @object)
        {
            return Assert.IsType<T>(@object);
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="expectedType">The type the object should be</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void ShouldBeType(this object @object,
                                        Type expectedType)
        {
            Assert.IsType(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type
        /// </summary>
        /// <typeparam name="T">The type the object should implement</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T ShouldImplement<T>(this object @object)
        {
            return Assert.IsAssignableFrom<T>(@object);
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type
        /// </summary>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="expectedType">The type the object should implement</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void ShouldImplement(this object @object,
                                           Type expectedType)
        {
            Assert.IsAssignableFrom(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type
        /// </summary>
        /// <typeparam name="T">The type the object should implement</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="message">The user message to show on failure</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T ShouldImplement<T>(this object @object, string userMessage)
        {
            return Assert.IsAssignableFrom<T>(@object, userMessage);
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type
        /// </summary>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="expectedType">The type the object should implement</param>
        /// <param name="message">The user message to show on failure</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void ShouldImplement(this object @object,
                                           Type expectedType,
                                           string userMessage)
        {
            Assert.IsAssignableFrom(expectedType, @object, userMessage);
        }

        /// <summary>
        /// Verifies that two objects are equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual<T>(this T actual,
                                          T expected)
        {
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verifies that two objects are equal, using a default comparer, with a custom error message
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="message">The user message to show on failure</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual<T>(this T actual,
                                          T expected,
                                          string userMessage)
        {
            Assert.Equal(expected, actual, userMessage);
        }

        /// <summary>
        /// Verifies that two objects are equal, using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="comparer">The comparer used to compare the two objects</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual<T>(this T actual,
                                          T expected,
                                          IEqualityComparer<T> comparer)
        {
            Assert.Equal(expected, actual, comparer);
        }

        /// <summary>
        /// Verifies that a value is not within a given range, using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <exception cref="NotInRangeException">Thrown when the value is in the given range</exception>
        public static void ShouldNotBeInRange<T>(this T actual,
                                                 T low,
                                                 T high)
        {
            Assert.NotInRange(actual, low, high);
        }

        /// <summary>
        /// Verifies that a value is not within a given range, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <param name="comparer">The comparer used to evaluate the value's range</param>
        /// <exception cref="NotInRangeException">Thrown when the value is in the given range</exception>
        public static void ShouldNotBeInRange<T>(this T actual,
                                                 T low,
                                                 T high,
                                                 IComparer<T> comparer)
        {
            Assert.NotInRange(actual, low, high, comparer);
        }

        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="object">The object to be validated</param>
        /// <exception cref="NotNullException">Thrown when the object is not null</exception>
        public static T ShouldNotBeNull<T>(this T @object) where T : class
        {
            Assert.NotNull(@object);
            return @object;
        }

        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="object">The object to be validated</param>
        /// <param name="message">The message to show on failure</param>
        /// <exception cref="NotNullException">Thrown when the object reference is null</exception>
        public static T ShouldNotBeNull<T>(this T @object, string message) where T : class
        {
            Assert.NotNull(@object, message);
            return @object;
        }


        /// <summary>
        /// Verifies that two objects are not the same instance.
        /// </summary>
        /// <param name="actual">The actual object instance</param>
        /// <param name="expected">The expected object instance</param>
        /// <exception cref="NotSameException">Thrown when the objects are the same instance</exception>
        public static void ShouldNotBeSameAs(this object actual,
                                             object expected)
        {
            Assert.NotSame(expected, actual);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <typeparam name="T">The type the object should not be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is the given type</exception>
        public static void ShouldNotBeType<T>(this object @object)
        {
            Assert.IsNotType<T>(@object);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="expectedType">The type the object should not be</param>
        /// <exception cref="IsTypeException">Thrown when the object is the given type</exception>
        public static void ShouldNotBeType(this object @object,
                                           Type expectedType)
        {
            Assert.IsNotType(expectedType, @object);
        }

        /// <summary>
        /// Verifies that two objects are not equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The actual object</param>
        /// <param name="expected">The expected object</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void ShouldNotEqual<T>(this T actual,
                                             T expected)
        {
            Assert.NotEqual(expected, actual);
        }

        /// <summary>
        /// Verifies that two objects are not equal, using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The actual object</param>
        /// <param name="expected">The expected object</param>
        /// <param name="comparer">The comparer used to examine the objects</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void ShouldNotEqual<T>(this T actual,
                                             T expected,
                                             IEqualityComparer<T> comparer)
        {
            Assert.NotEqual(expected, actual, comparer);
        }
    }
}