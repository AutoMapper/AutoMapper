using System;
using System.Collections;
using System.Collections.Generic;
using Should.Core.Exceptions;

namespace Should.Core.Assertions
{
    /// <summary>
    /// A wrapper for Assert which is used by <see cref="TestClass"/>.
    /// </summary>
    public class Assertions
    {
        /// <summary>
        /// Verifies that a collection contains a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="expected">The object expected to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the collection</exception>
        public void Contains<T>(T expected, IEnumerable<T> collection)
        {
            Assert.Contains(expected, collection);
        }

        /// <summary>
        /// Verifies that a collection contains a given object, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="expected">The object expected to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="comparer">The comparer used to equate objects in the collection with the expected object</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the collection</exception>
        public void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            Assert.Contains(expected, collection, comparer);
        }

        /// <summary>
        /// Verifies that a string contains a given sub-string, using the current culture.
        /// </summary>
        /// <param name="expectedSubString">The sub-string expected to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
        public void Contains(string expectedSubString, string actualString)
        {
            Assert.Contains(expectedSubString, actualString);
        }

        /// <summary>
        /// Verifies that a string contains a given sub-string, using the given comparison type.
        /// </summary>
        /// <param name="expectedSubString">The sub-string expected to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
        public void Contains(string expectedSubString, string actualString, StringComparison comparisonType)
        {
            Assert.Contains(expectedSubString, actualString, comparisonType);
        }

        /// <summary>
        /// Verifies that a collection does not contain a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container</exception>
        public void DoesNotContain<T>(T expected, IEnumerable<T> collection)
        {
            Assert.DoesNotContain(expected, collection);
        }

        /// <summary>
        /// Verifies that a collection does not contain a given object, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="comparer">The comparer used to equate objects in the collection with the expected object</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container</exception>
        public void DoesNotContain<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            Assert.DoesNotContain(expected, collection, comparer);
        }

        /// <summary>
        /// Verifies that a string does not contain a given sub-string, using the current culture.
        /// </summary>
        /// <param name="expectedSubString">The sub-string which is expected not to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the string</exception>
        public void DoesNotContain(string expectedSubString, string actualString)
        {
            Assert.DoesNotContain(expectedSubString, actualString);
        }

        /// <summary>
        /// Verifies that a string does not contain a given sub-string, using the current culture.
        /// </summary>
        /// <param name="expectedSubString">The sub-string which is expected not to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the given string</exception>
        public void DoesNotContain(string expectedSubString, string actualString, StringComparison comparisonType)
        {
            Assert.DoesNotContain(expectedSubString, actualString, comparisonType);
        }

        ///// <summary>
        ///// Verifies that a block of code does not throw any exceptions.
        ///// </summary>
        ///// <param name="testCode">A delegate to the code to be tested</param>
        //public void DoesNotThrow(Assert.ThrowsDelegate testCode)
        //{
        //    Assert.DoesNotThrow(testCode);
        //}

        /// <summary>
        /// Verifies that a collection is empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null</exception>
        /// <exception cref="EmptyException">Thrown when the collection is not empty</exception>
        public void Empty(IEnumerable collection)
        {
            Assert.Empty(collection);
        }

        /// <summary>
        /// Verifies that two objects are equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public void Equal<T>(T expected, T actual)
        {
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verifies that two objects are equal, using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="comparer">The comparer used to compare the two objects</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public void Equal<T>(T expected, T actual, IEqualityComparer<T> comparer)
        {
            Assert.Equal(expected, actual, comparer);
        }

        /// <summary>Do not call this method. Call Assert.Equal() instead.</summary>
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public void False(bool condition)
        {
            Assert.False(condition);
        }

        /// <summary>
        /// Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <param name="userMessage">The message to show when the condition is not false</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public void False(bool condition, string userMessage)
        {
            Assert.False(condition, userMessage);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object"/>.</returns>
        public override int GetHashCode()
        {
            return 42;
        }

        /// <summary>Verifies that an object is greater than the exclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="maxValue">An object representing the exclusive minimum value of paramref name="value"/>.</param>
        public static void GreaterThan<T>(T value, T maxValue)
        {
            Assert.GreaterThan(value, maxValue);
        }

        /// <summary>Verifies that an object is greater than the exclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="minValue">An object representing the exclusive minimum value of paramref name="value"/>.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void GreaterThan<T>(T value, T minValue, IComparer<T> comparer)
        {
            Assert.GreaterThan(value, minValue, comparer);
        }

        /// <summary>Verifies that an object is greater than the inclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="minValue">An object representing the inclusive minimum value of paramref name="value"/>.</param>
        public static void GreaterThanOrEqual<T>(T value, T minValue)
        {
            Assert.GreaterThanOrEqual(value, minValue);
        }

        /// <summary>Verifies that an object is greater than the inclusive minimum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="minValue">An object representing the inclusive minimum value of paramref name="value"/>.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void GreaterThanOrEqual<T>(T value, T minValue, IComparer<T> comparer)
        {
            Assert.GreaterThanOrEqual(value, minValue, comparer);
        }

        /// <summary>
        /// Verifies that a value is within a given range.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <exception cref="InRangeException">Thrown when the value is not in the given range</exception>
        public void InRange<T>(T actual, T low, T high)
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
        public void InRange<T>(T actual, T low, T high, IComparer<T> comparer)
        {
            Assert.InRange(actual, low, high, comparer);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <typeparam name="T">The type the object should not be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is the given type</exception>
        public void IsNotType<T>(object @object)
        {
            Assert.IsNotType<T>(@object);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <param name="expectedType">The type the object should not be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is the given type</exception>
        public void IsNotType(Type expectedType, object @object)
        {
            Assert.IsNotType(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public T IsType<T>(object @object)
        {
            return Assert.IsType<T>(@object);
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <param name="expectedType">The type the object should be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public void IsType(Type expectedType, object @object)
        {
            Assert.IsType(expectedType, @object);
        }

        /// <summary>Verifies that an object is less than the exclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="maxValue">An object representing the exclusive maximum value of paramref name="value"/>.</param>
        public static void LessThan<T>(T value, T maxValue)
        {
            Assert.LessThan(value, maxValue);
        }

        /// <summary>Verifies that an object is less than the exclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="maxValue">An object representing the exclusive maximum value of paramref name="value"/>.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void LessThan<T>(T value, T maxValue, IComparer<T> comparer)
        {
            Assert.LessThan(value, maxValue, comparer);
        }

        /// <summary>Verifies that an object is less than the inclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="maxValue">An object representing the inclusive maximum value of paramref name="value"/>.</param>
        public static void LessThanOrEqual<T>(T value, T maxValue)
        {
            Assert.LessThanOrEqual(value, maxValue);
        }

        /// <summary>Verifies that an object is less than the inclusive maximum value.</summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="value">The object to be evaluated.</param>
        /// <param name="maxValue">An object representing the inclusive maximum value of paramref name="value"/>.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> used to compare the objects.</param>
        public static void LessThanOrEqual<T>(T value, T maxValue, IComparer<T> comparer)
        {
            Assert.LessThanOrEqual(value, maxValue, comparer);
        }

        /// <summary>
        /// Verifies that a collection is not empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when a null collection is passed</exception>
        /// <exception cref="NotEmptyException">Thrown when the collection is empty</exception>
        public void NotEmpty(IEnumerable collection)
        {
            Assert.NotEmpty(collection);
        }

        /// <summary>
        /// Verifies that two objects are not equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public void NotEqual<T>(T expected, T actual)
        {
            Assert.NotEqual(expected, actual);
        }

        /// <summary>
        /// Verifies that two objects are not equal, using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="comparer">The comparer used to examine the objects</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public void NotEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
        {
            Assert.NotEqual(expected, actual, comparer);
        }

        /// <summary>
        /// Verifies that a value is not within a given range, using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <exception cref="NotInRangeException">Thrown when the value is in the given range</exception>
        public void NotInRange<T>(T actual, T low, T high)
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
        public void NotInRange<T>(T actual, T low, T high, IComparer<T> comparer)
        {
            Assert.NotInRange(actual, low, high, comparer);
        }

        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="object">The object to be validated</param>
        /// <exception cref="NotNullException">Thrown when the object is not null</exception>
        public void NotNull(object @object)
        {
            Assert.NotNull(@object);
        }

        /// <summary>
        /// Verifies that two objects are not the same instance.
        /// </summary>
        /// <param name="expected">The expected object instance</param>
        /// <param name="actual">The actual object instance</param>
        /// <exception cref="NotSameException">Thrown when the objects are the same instance</exception>
        public void NotSame(object expected, object actual)
        {
            Assert.NotSame(expected, actual);
        }

        /// <summary>
        /// Verifies that an object reference is null.
        /// </summary>
        /// <param name="object">The object to be inspected</param>
        /// <exception cref="NullException">Thrown when the object reference is not null</exception>
        public void Null(object @object)
        {
            Assert.Null(@object);
        }

        /// <summary>
        /// Verifies that two objects are the same instance.
        /// </summary>
        /// <param name="expected">The expected object instance</param>
        /// <param name="actual">The actual object instance</param>
        /// <exception cref="SameException">Thrown when the objects are not the same instance</exception>
        public void Same(object expected, object actual)
        {
            Assert.Same(expected, actual);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public T Throws<T>(Assert.ThrowsDelegate testCode)
            where T : Exception
        {
            return Assert.Throws<T>(testCode);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="userMessage">The message to be shown if the test fails</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public T Throws<T>(string userMessage, Assert.ThrowsDelegate testCode)
            where T : Exception
        {
            return Assert.Throws<T>(userMessage, testCode);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public Exception Throws(Type exceptionType, Assert.ThrowsDelegate testCode)
        {
            return Assert.Throws(exceptionType, testCode);
        }

        /// <summary>
        /// Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public void True(bool condition)
        {
            Assert.True(condition);
        }

        /// <summary>
        /// Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <param name="userMessage">The message to be shown when the condition is false</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public void True(bool condition, string userMessage)
        {
            Assert.True(condition, userMessage);
        }
    }
}