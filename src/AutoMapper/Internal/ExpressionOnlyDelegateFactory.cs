using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    public class DelegateFactoryOverride : DelegateFactory
    {
        public override LateBoundFieldSet CreateSet(FieldInfo field)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);
            BinaryExpression assignExpression = Expression.Assign(member, Expression.Convert(valueParameter, field.FieldType));

            Expression<LateBoundFieldSet> lambda = Expression.Lambda<LateBoundFieldSet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );

            return lambda.Compile();
        }

        public override LateBoundPropertySet CreateSet(PropertyInfo property)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            MemberExpression member = Expression.Property(Expression.Convert(instanceParameter, property.DeclaringType), property);
            BinaryExpression assignExpression = Expression.Assign(member, Expression.Convert(valueParameter, property.PropertyType));

            Expression<LateBoundPropertySet> lambda = Expression.Lambda<LateBoundPropertySet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );


            return lambda.Compile();
        }
    }
}
