using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ContosoUniversity.Utils.Structures;

namespace ContosoUniversity.Utils
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Creates an OrderBy expression from a SortCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortCollection"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> BuildOrderByExpression<T>(this SortCollection sortCollection) where T : class
        {
            if (sortCollection == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>));
            MethodCallExpression mce = param.GetOrderBy<T>(sortCollection);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(mce, param);
        }

        /// <summary>
        /// Creates an order by expression for a parameter expression
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="sorts"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetOrderBy<TSource>(this ParameterExpression expression, SortCollection sorts, string parameterName = "rr")
        {
            Dictionary<string, SortDescription> sortsDictionary = sorts.SortDescriptions.ToDictionary(item => item.PropertyName);

            PropertyInfo[] properties = typeof(TSource).GetTypeInfo().GetProperties();
            MethodCallExpression resultExp = null;
            int count = 0;
            foreach (SortDescription description in sorts.SortDescriptions)
            {
                var param = Expression.Parameter(typeof(TSource), parameterName);
                var parts = description.PropertyName.Split('.');
                PropertyInfo info = GetPropertyInfo(typeof(TSource), description.PropertyName);
                Expression parent = param;
                foreach (var part in parts)
                    parent = Expression.Property(parent, part);

                var typeArgs = new[] { typeof(TSource), info.PropertyType };
                var delegateType = typeof(Func<,>).MakeGenericType(typeArgs);
                var typedProperty = Expression.Lambda(delegateType, parent, param);

                count++;
                if (count == 1)
                {
                    resultExp = description.SortDirection == ListSortDirection.Ascending
                        ? Expression.Call(typeof(Queryable), "OrderBy", typeArgs, expression, typedProperty)
                        : Expression.Call(typeof(Queryable), "OrderByDescending", typeArgs, expression, typedProperty);
                }
                else
                {
                    resultExp = description.SortDirection == ListSortDirection.Ascending
                        ? Expression.Call(typeof(Queryable), "ThenBy", typeArgs, resultExp, typedProperty)
                        : Expression.Call(typeof(Queryable), "ThenByDescending", typeArgs, resultExp, typedProperty);
                }
            }

            resultExp = Expression.Call(typeof(Queryable), "Skip", new[] { typeof(TSource) }, resultExp, Expression.Constant(sorts.Skip));
            resultExp = Expression.Call(typeof(Queryable), "Take", new[] { typeof(TSource) }, resultExp, Expression.Constant(sorts.Take));

            return resultExp;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyFullName)
        {
            if (propertyFullName.IndexOf('.') < 0)
            {
                PropertyInfo propertyInfo = type.GetTypeInfo().GetProperties().FirstOrDefault(p => p.Name == propertyFullName);
                if (propertyInfo == null)
                    throw new ArgumentNullException(string.Format("Property {0}, does not exists in type {1}", propertyFullName, type.FullName));

                return propertyInfo;
            }

            string propertyName = propertyFullName.Substring(0, propertyFullName.IndexOf('.'));
            string childFullName = propertyFullName.Substring(propertyFullName.IndexOf('.') + 1);

            return GetPropertyInfo(type.GetTypeInfo().GetProperty(propertyName).PropertyType, childFullName);
        }
    }
}
