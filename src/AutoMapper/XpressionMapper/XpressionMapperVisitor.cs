using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AutoMapper.Configuration;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions.Impl;
using AutoMapper.XpressionMapper.ArgumentMappers;
using AutoMapper.XpressionMapper.Extensions;
using AutoMapper.XpressionMapper.Structures;

namespace AutoMapper.XpressionMapper
{
    public class XpressionMapperVisitor : ExpressionVisitor
    {
        public XpressionMapperVisitor(IConfigurationProvider configurationProvider, Dictionary<Type, Type> typeMappings)
        {
            TypeMappings = typeMappings;
            InfoDictionary = new MapperInfoDictionary(new ParameterExpressionEqualityComparer());
            ConfigurationProvider = configurationProvider;
        }

        public MapperInfoDictionary InfoDictionary { get; }

        public Dictionary<Type, Type> TypeMappings { get; }

        protected IConfigurationProvider ConfigurationProvider { get; }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            InfoDictionary.Add(parameterExpression, TypeMappings);
            var pair = InfoDictionary.SingleOrDefault(a => a.Key.Equals(parameterExpression));
            return !pair.Equals(default(KeyValuePair<Type, MapperInfo>)) ? pair.Value.NewParameter : base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            string sourcePath;

            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, TypeMappings);

            var sType = parameterExpression.Type;
            if (InfoDictionary.ContainsKey(parameterExpression) && node.IsMemberExpression())
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

            if (propertyMapInfoList.Any(x => x.CustomExpression != null))
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

                this.TypeMappings.AddTypeMapping(node.Type, ex.Type);
                return ex;
            }
            fullName = BuildFullName(propertyMapInfoList);
            var me = ExpressionFactory.MemberAccesses(fullName, InfoDictionary[parameterExpression].NewParameter);

            this.TypeMappings.AddTypeMapping(node.Type, me.Type);
            return me;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (this.TypeMappings.TryGetValue(node.Type, out Type newType))
                return base.VisitConstant(Expression.Constant(node.Value, newType));

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMethodCall(node);

            InfoDictionary.Add(parameterExpression, TypeMappings);

            var listOfArgumentsForNewMethod = node.Arguments.Aggregate(new List<Expression>(), (lst, next) =>
            {
                var mappedNext = ArgumentMapper.Create(this, next).MappedArgumentExpression;
                TypeMappings.AddTypeMapping(next.Type, mappedNext.Type);

                lst.Add(mappedNext);
                return lst;
            });//Arguments could be expressions or other objects. e.g. s => s.UserId  or a string "ZZZ".  For extention methods node.Arguments[0] is usually the helper object itself

            //type args are the generic type args e.g. T1 and T2 MethodName<T1, T2>(method arguments);
            var typeArgsForNewMethod = node.Method.IsGenericMethod
                ? node.Method.GetGenericArguments().Select(i => TypeMappings.ContainsKey(i) ? TypeMappings[i] : i).ToList()//not converting the type it is not in the typeMappings dictionary
                : null;

            MethodCallExpression resultExp;
            if (!node.Method.IsStatic)
            {
                var instance = ArgumentMapper.Create(this, node.Object).MappedArgumentExpression;

                resultExp = node.Method.IsGenericMethod
                    ? Expression.Call(instance, node.Method.Name, typeArgsForNewMethod.ToArray(), listOfArgumentsForNewMethod.ToArray())
                    : Expression.Call(instance, node.Method, listOfArgumentsForNewMethod.ToArray());
            }
            else
            {
                resultExp = node.Method.IsGenericMethod
                    ? Expression.Call(node.Method.DeclaringType, node.Method.Name, typeArgsForNewMethod.ToArray(), listOfArgumentsForNewMethod.ToArray())
                    : Expression.Call(node.Method, listOfArgumentsForNewMethod.ToArray());
            }

            return resultExp;
        }

        protected string BuildFullName(List<PropertyMapInfo> propertyMapInfoList)
        {
            var fullName = string.Empty;
            foreach (var info in propertyMapInfoList)
            {
                if (info.CustomExpression != null)
                {
                    fullName = string.IsNullOrEmpty(fullName)
                        ? info.CustomExpression.GetMemberFullName()
                        : string.Concat(fullName, ".", info.CustomExpression.GetMemberFullName());
                }
                else
                {
                    var additions = info.DestinationPropertyInfos.Aggregate(new StringBuilder(fullName), (sb, next) =>
                    {
                        if (sb.ToString() == string.Empty)
                            sb.Append(next.Name);
                        else
                        {
                            sb.Append(".");
                            sb.Append(next.Name);
                        }
                        return sb;
                    });

                    fullName = additions.ToString();
                }
            }

            return fullName;
        }

        private static void AddPropertyMapInfo(Type parentType, string name, List<PropertyMapInfo> propertyMapInfoList)
        {
            var sourceMemberInfo = parentType.GetFieldOrProperty(name);
            switch (sourceMemberInfo)
            {
                case PropertyInfo propertyInfo:
                    propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> {propertyInfo}));
                    break;
                case FieldInfo fieldInfo:
                    propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> {fieldInfo}));
                    break;
            }
        }

        protected void FindDestinationFullName(Type typeSource, Type typeDestination, string sourceFullName, List<PropertyMapInfo> propertyMapInfoList)
        {
            const string period = ".";
            if (typeSource == typeDestination)
            {
                var sourceFullNameArray = sourceFullName.Split(new[] { period[0] }, StringSplitOptions.RemoveEmptyEntries);
                sourceFullNameArray.Aggregate(propertyMapInfoList, (list, next) =>
                {

                    if (list.Count == 0)
                    {
                        AddPropertyMapInfo(typeSource,  next, list);
                    }
                    else
                    {
                        var last = list[list.Count - 1];
                        AddPropertyMapInfo(last.CustomExpression == null
                            ? last.DestinationPropertyInfos[last.DestinationPropertyInfos.Count - 1].GetMemberType()
                            : last.CustomExpression.ReturnType, next, list);
                    }
                    return list;
                });
                return;
            }

            var typeMap = ConfigurationProvider.CheckIfMapExists(sourceType: typeDestination, destinationType: typeSource);//The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination 

            PathMap pathMap = typeMap.FindPathMapByDestinationPath(destinationFullPath: sourceFullName);
            if (pathMap != null)
            {
                propertyMapInfoList.Add(new PropertyMapInfo(pathMap.SourceExpression, new List<MemberInfo>()));
                return;
            }


            if (sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) < 0)
            {
                var propertyMap = typeMap.GetPropertyMapByDestinationProperty(sourceFullName);
                var sourceMemberInfo = typeSource.GetFieldOrProperty(propertyMap.DestinationProperty.Name);
                if (propertyMap.ValueResolverConfig != null)
                {
                    throw new InvalidOperationException(Resource.customResolversNotSupported);
                }
                if (propertyMap.CustomExpression != null)
                {
                    if (propertyMap.CustomExpression.ReturnType.IsValueType() && sourceMemberInfo.GetMemberType() != propertyMap.CustomExpression.ReturnType)
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.expressionMapValueTypeMustMatchFormat, propertyMap.CustomExpression.ReturnType.Name, propertyMap.CustomExpression.ToString(), sourceMemberInfo.GetMemberType().Name, propertyMap.DestinationProperty.Name));
                }
                else
                {
                    if (propertyMap.SourceMember.GetMemberType().IsValueType() && sourceMemberInfo.GetMemberType() != propertyMap.SourceMember.GetMemberType())
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.expressionMapValueTypeMustMatchFormat, propertyMap.SourceMember.GetMemberType().Name, propertyMap.SourceMember.Name, sourceMemberInfo.GetMemberType().Name, propertyMap.DestinationProperty.Name));
                }

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomExpression, propertyMap.SourceMembers.ToList()));
            }
            else
            {
                var propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase));
                var propertyMap = typeMap.GetPropertyMapByDestinationProperty(propertyName);

                var sourceMemberInfo = typeSource.GetFieldOrProperty(propertyMap.DestinationProperty.Name);
                if (propertyMap.CustomExpression == null && propertyMap.SourceMember == null)//If sourceFullName has a period then the SourceMember cannot be null.  The SourceMember is required to find the ProertyMap of its child object.
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, propertyName));

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomExpression, propertyMap.SourceMembers.ToList()));
                var childFullName = sourceFullName.Substring(sourceFullName.IndexOf(period, StringComparison.OrdinalIgnoreCase) + 1);

                FindDestinationFullName(sourceMemberInfo.GetMemberType(), propertyMap.CustomExpression == null
                    ? propertyMap.SourceMember.GetMemberType()
                    : propertyMap.CustomExpression.ReturnType, childFullName, propertyMapInfoList);
            }
        }
    }
}