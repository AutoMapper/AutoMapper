#if NETSTANDARD1_3 || NET45
namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Collections.Specialized;

    public class NameValueCollectionMapper : IObjectMapper
    {
        private static NameValueCollection Map(NameValueCollection source)
        {
            if (source == null)
                return null;

            var nvc = new NameValueCollection();
            foreach (var s in source.AllKeys)
                nvc.Add(s, source[s]);

            return nvc;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(NameValueCollectionMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(TypePair context)
        {
            return
                context.SourceType == typeof (NameValueCollection) &&
                context.DestinationType == typeof (NameValueCollection);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo, sourceExpression);
        }
    }
}
#endif