﻿using System.Collections.Specialized;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using System.Reflection;

#if !NETSTANDARD1_1
namespace AutoMapper.Mappers
{
    public class NameValueCollectionMapper : IObjectMapper
    {
        private static NameValueCollection Map(NameValueCollection source)
        {
            var nvc = new NameValueCollection();
            foreach (var s in source.AllKeys)
                nvc.Add(s, source[s]);

            return nvc;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(NameValueCollectionMapper).GetDeclaredMethod(nameof(Map));

        public bool IsMatch(in TypePair context) => context.SourceType == typeof (NameValueCollection) &&
                                                 context.DestinationType == typeof (NameValueCollection);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression) => 
            Call(null, MapMethodInfo, sourceExpression);
    }
}
#endif