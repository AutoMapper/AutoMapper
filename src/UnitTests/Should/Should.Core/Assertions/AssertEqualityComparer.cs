using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Should.Core.Assertions
{
    using AutoMapper;

    internal class AssertEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            Type type = typeof(T);

            // Null?
            if (!type.IsValueType() || (type.IsGenericType() && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (Object.Equals(x, default(T)))
                    return Object.Equals(y, default(T));

                if (Object.Equals(y, default(T)))
                    return false;
            }

            //x implements IEquitable<T> and is assignable from y?
            var xIsAssignableFromY = x.GetType().IsAssignableFrom(y.GetType());
            if (xIsAssignableFromY && x is IEquatable<T>)
                return ((IEquatable<T>)x).Equals(y);

            //y implements IEquitable<T> and is assignable from x?
            var yIsAssignableFromX = y.GetType().IsAssignableFrom(x.GetType());
            if (yIsAssignableFromX && y is IEquatable<T>)
                return ((IEquatable<T>)y).Equals(x);

            // Enumerable?
            IEnumerable enumerableX = x as IEnumerable;
            IEnumerable enumerableY = y as IEnumerable;

            if (enumerableX != null && enumerableY != null)
            {
                return new EnumerableEqualityComparer().Equals(enumerableX, enumerableY);
            }

            // Last case, rely on Object.Equals
            return Object.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }
}