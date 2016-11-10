namespace AutoMapper.Mappers
{
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;

    public class StringMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.DestinationType == typeof(string) && context.SourceType != typeof(string);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression)
        {
            return Condition(Equal(sourceExpression, Default(sourceExpression.Type)),
                Constant(null, typeof(string)),
                Call(sourceExpression, typeof(object).GetDeclaredMethod("ToString")));
        }
    }
}