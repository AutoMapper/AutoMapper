using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.XpressionMapper.Extensions;
using AutoMapper.XpressionMapper.Structures;
using System.Linq;

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
            MemberExpression me;
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:

                    me = ((node != null) ? node.Operand : null) as MemberExpression;
                    ParameterExpression parameterExpression = node.GetParameterExpression();
                    Type sType = parameterExpression == null ? null : parameterExpression.Type;
                    if (sType != null && me.Expression.NodeType == ExpressionType.MemberAccess && (me.Type == typeof(string) || me.Type.GetTypeInfo().IsValueType || (me.Type.GetTypeInfo().IsGenericType
                                                                                                                                    && me.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                                                                                                                                    && Nullable.GetUnderlyingType(me.Type).GetTypeInfo().IsValueType)))
                    {
                        //ParameterExpression parameter = me.Expression.GetParameter();
                        //string fullName = me.Expression.GetPropertyFullName();
                        //return parameter.BuildExpression(sType, fullName);
                        return this.Visit(me.Expression);
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
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath = null;

            ParameterExpression parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, this.TypeMappings);
            Type sType = parameterExpression.Type;
            if (sType != null && InfoDictionary.ContainsKey(parameterExpression) && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            List<PropertyMapInfo> propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, InfoDictionary[parameterExpression].DestType, sourcePath, propertyMapInfoList);
            string fullName = null;

            if (propertyMapInfoList.Any(x => x.CustomExpression != null))//CustomExpression takes precedence over DestinationPropertyInfo
            {
                PropertyMapInfo last = propertyMapInfoList.Last(x => x.CustomExpression != null);
                List<PropertyMapInfo> beforeCustExpression = propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) < propertyMapInfoList.IndexOf(last))
                        list.Add(next);
                    return list;
                });

                List<PropertyMapInfo> afterCustExpression = propertyMapInfoList.Aggregate(new List<PropertyMapInfo>(), (list, next) =>
                {
                    if (propertyMapInfoList.IndexOf(next) > propertyMapInfoList.IndexOf(last))
                        list.Add(next);
                    return list;
                });


                fullName = BuildFullName(beforeCustExpression);

                PrependParentNameVisitor visitor = new PrependParentNameVisitor(last.CustomExpression.Parameters[0].Type/*Parent type of current property*/, fullName, InfoDictionary[parameterExpression].NewParameter);

                Expression ex = propertyMapInfoList[propertyMapInfoList.Count - 1] != last
                    ? visitor.Visit(last.CustomExpression.Body.AddExpressions(afterCustExpression))
                    : visitor.Visit(last.CustomExpression.Body);

                FindMemberExpressionsVisitor v = new FindMemberExpressionsVisitor(InfoDictionary[parameterExpression].NewParameter);
                v.Visit(ex);

                return v.Result;
            }
            else
            {
                fullName = BuildFullName(propertyMapInfoList);
                MemberExpression me = InfoDictionary[parameterExpression].NewParameter.BuildExpression(fullName);
                if (me.Expression.NodeType == ExpressionType.MemberAccess && (me.Type == typeof(string) || me.Type.GetTypeInfo().IsValueType || (me.Type.GetTypeInfo().IsGenericType
                                                                                                                            && me.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                                                                                                                            && Nullable.GetUnderlyingType(me.Type).GetTypeInfo().IsValueType)))
                {
                    return me.Expression;
                }

                return me;
            }
        }
    }
}
