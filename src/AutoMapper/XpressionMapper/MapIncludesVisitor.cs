using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.XpressionMapper.Extensions;
using AutoMapper.XpressionMapper.Structures;

namespace AutoMapper.XpressionMapper
{
    public class MapIncludesVisitor : XpressionMapperVisitor
    {
        public MapIncludesVisitor(IConfigurationProvider configurationProvider, Dictionary<Type, Type> typeMappings)
            : base(configurationProvider, typeMappings)
        {
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:

                    var me = node.Operand as MemberExpression;
                    var parameterExpression = node.GetParameterExpression();
                    var sType = parameterExpression?.Type;
                    if (me != null && (sType != null && me.Expression.NodeType == ExpressionType.MemberAccess && me.Type.IsLiteralType()))
                    {
                        //just pass me and let the FindMemberExpressionsVisitor handle removing of the value type
                        //me.Expression will not match the PathMap name.
                        return Visit(me);
                    }
                    else
                    {
                        return base.VisitUnary(node);
                    }
                default:
                    return base.VisitUnary(node);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            string sourcePath;

            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, TypeMappings);
            var sType = parameterExpression.Type;
            if (sType != null && InfoDictionary.ContainsKey(parameterExpression) && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            var propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, InfoDictionary[parameterExpression].DestType, sourcePath, propertyMapInfoList);
            string fullName;

            if (propertyMapInfoList.Any(x => x.CustomExpression != null))//CustomExpression takes precedence over DestinationPropertyInfo
            {
                var last = propertyMapInfoList.Last(x => x.CustomExpression != null);
                var beforeCustExpression = propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) < propertyMapInfoList.IndexOf(last))
                        list.Add(next);
                    return list;
                });

                var afterCustExpression = propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) > propertyMapInfoList.IndexOf(last))
                        list.Add(next);
                    return list;
                });


                fullName = BuildFullName(beforeCustExpression);

                var visitor = new PrependParentNameVisitor(last.CustomExpression.Parameters[0].Type/*Parent type of current property*/, fullName, InfoDictionary[parameterExpression].NewParameter);

                var ex = propertyMapInfoList[propertyMapInfoList.Count - 1] != last
                    ? visitor.Visit(last.CustomExpression.Body.MemberAccesses(afterCustExpression))
                    : visitor.Visit(last.CustomExpression.Body);

                var v = new FindMemberExpressionsVisitor(InfoDictionary[parameterExpression].NewParameter);
                v.Visit(ex);

                return v.Result;
            }
            fullName = BuildFullName(propertyMapInfoList);
            var me = ExpressionFactory.MemberAccesses(fullName, InfoDictionary[parameterExpression].NewParameter);
            if (me.Expression.NodeType == ExpressionType.MemberAccess && (me.Type == typeof(string) || me.Type.GetTypeInfo().IsValueType || (me.Type.GetTypeInfo().IsGenericType
                                                                                                                                             && me.Type.GetGenericTypeDefinition() == typeof(Nullable<>)
                                                                                                                                             && Nullable.GetUnderlyingType(me.Type).GetTypeInfo().IsValueType)))
            {
                return me.Expression;
            }

            return me;
        }
    }
}
