using System;
using System.Collections.Generic;
using System.Reflection;

namespace Should.Core.Assertions
{
    using AutoMapper;

    internal class AssertComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            Type type = typeof(T);

            // Null?
            if (!type.IsValueType() || (type.IsGenericType() && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (Equals(x, default(T)))
                {
                    return Equals(y, default(T)) ? 0 : 1;
                }

                if (Equals(y, default(T)))
                    return -1;
            }

            var xIsAssignableFromY = x.GetType().IsAssignableFrom(y.GetType());
            var yIsAssignableFromX = y.GetType().IsAssignableFrom(x.GetType());

            if (!xIsAssignableFromY && !yIsAssignableFromX)
                throw new InvalidOperationException(string.Format("Cannot compare objects of type {0} and {1} because neither is assignable from the other.", x.GetType().Name, y.GetType().Name));

            // x Implements IComparable<T>?
            IComparable<T> comparable1 = x as IComparable<T>;

            if (comparable1 != null && xIsAssignableFromY)
                return comparable1.CompareTo(y);

            // y Implements IComparable<T>?
            IComparable<T> comparable2 = y as IComparable<T>;

            if (comparable2 != null && yIsAssignableFromX)
                return comparable2.CompareTo(x) * -1;

            // x Implements IComparable?
            IComparable comparable3 = x as IComparable;

            if (comparable3 != null && xIsAssignableFromY)
                return comparable3.CompareTo(y);

            // y Implements IComparable?
            IComparable comparable4 = y as IComparable;

            if (comparable4 != null && yIsAssignableFromX)
                return comparable4.CompareTo(x) *-1;

            if (new AssertEqualityComparer<T>().Equals(x, y))
            {
                return 0;
            }

            if (xIsAssignableFromY)
            {
                var result = CompareUsingOperators(x, y, x.GetType());
                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            if (yIsAssignableFromX)
            {
                var result = CompareUsingOperators(x, y, y.GetType());
                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            throw new InvalidOperationException(string.Format("Cannot compare objects of type {0} and {1} because neither implements IComparable or IComparable<T> nor overloads comparison operators.", x.GetType().Name, y.GetType().Name));
        }

        //Note: Handles edge case of a class where operators are overloaded but niether IComparable or IComparable<T> are implemented
        private int? CompareUsingOperators(T x, T y, Type type)
        {
            var greaterThan = type.GetMethod("op_GreaterThan");
            if (greaterThan != null)
            {
                var lessThan = type.GetMethod("op_LessThan");
                return (bool)greaterThan.Invoke(null, new object[] { x, y })
                    ? 1
                    : (bool)lessThan.Invoke(null, new object[] { x, y }) ? -1 : 0;
            }
            var greaterThanOrEqual = type.GetMethod("op_GreaterThanOrEqual");
            if (greaterThanOrEqual != null)
            {
                var lessThanOrEqual = type.GetMethod("op_LessThanOrEqual");
                return (bool)greaterThanOrEqual.Invoke(null, new object[] { x, y })
                    ? (bool)lessThanOrEqual.Invoke(null, new object[] { x, y }) ? 0 : 1
                    : -1;
            }
            return null;
        }
    }
}