using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.XpressionMapper.ArgumentMappers;
using AutoMapper.XpressionMapper.Extensions;
using AutoMapper.XpressionMapper.Structures;
using System.Text;

namespace AutoMapper.XpressionMapper
{
    public class XpressionMapperVisitor : ExpressionVisitor
    {
        public XpressionMapperVisitor(IConfigurationProvider configurationProvider, Dictionary<Type, Type> typeMappings)
        {
            this.typeMappings = typeMappings;
            this.infoDictionary = new MapperInfoDictionary(new ParameterExpressionEqualityComparer());
            this.configurationProvider = configurationProvider;
        }

        #region Variables
        private MapperInfoDictionary infoDictionary;
        private Dictionary<Type, Type> typeMappings;
        private IConfigurationProvider configurationProvider;
        #endregion Variables

        #region Properties
        public MapperInfoDictionary InfoDictionary
        {
            get { return this.infoDictionary; }
        }

        public Dictionary<Type, Type> TypeMappings
        {
            get { return this.typeMappings; }
        }

        protected IConfigurationProvider ConfigurationProvider { get { return this.configurationProvider; } }
        #endregion Properties

        #region Methods
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            infoDictionary.Add(parameterExpression, this.TypeMappings);
            KeyValuePair<ParameterExpression, MapperInfo> pair = infoDictionary.SingleOrDefault(a => a.Key.Equals(parameterExpression));
            if (!pair.Equals(default(KeyValuePair<Type, MapperInfo>)))
            {
                return pair.Value.NewParameter;
            }

            return base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath = null;

            ParameterExpression parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            infoDictionary.Add(parameterExpression, this.TypeMappings);

            Type sType = parameterExpression.Type;
            if (infoDictionary.ContainsKey(parameterExpression) && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            List<PropertyMapInfo> propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, infoDictionary[parameterExpression].DestType, sourcePath, propertyMapInfoList);
            string fullName = null;

            if (propertyMapInfoList.Any(x => x.CustomExpression != null))
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
                PrependParentNameVisitor visitor = new PrependParentNameVisitor(last.CustomExpression.Parameters[0].Type/*Parent type of current property*/, fullName, infoDictionary[parameterExpression].NewParameter);

                Expression ex = propertyMapInfoList[propertyMapInfoList.Count - 1] != last
                    ? visitor.Visit(last.CustomExpression.Body.AddExpressions(afterCustExpression))
                    : visitor.Visit(last.CustomExpression.Body);

                return ex;
            }
            else
            {
                fullName = BuildFullName(propertyMapInfoList);
                MemberExpression me = infoDictionary[parameterExpression].NewParameter.BuildExpression(fullName);
                return me;
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var constantExpression = node.Right as ConstantExpression;
            if (constantExpression != null)
            {
                if (constantExpression.Value == null)
                {
                    if (node.Left.Type.GetTypeInfo().IsValueType)
                        return base.VisitBinary(node.Update(node.Left, node.Conversion, Expression.Constant(null, node.Left.Type)));
                    else
                        return base.VisitBinary(node.Update(node.Left, node.Conversion, Expression.Constant(null, typeof(object))));
                }
            }

            constantExpression = node.Left as ConstantExpression;
            if (constantExpression != null)
            {
                if (constantExpression.Value == null)
                {
                    if (node.Right.Type.GetTypeInfo().IsValueType)
                        return base.VisitBinary(node.Update(Expression.Constant(null, node.Right.Type), node.Conversion, node.Right));
                    else
                        return base.VisitBinary(node.Update(Expression.Constant(null, typeof(object)), node.Conversion, node.Right));
                }
            }

            Expression newLeft = this.Visit(node.Left);
            Expression newRight = this.Visit(node.Right);
            if ((newLeft.Type.GetTypeInfo().IsGenericType && newLeft.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) ^ (newRight.Type.GetTypeInfo().IsGenericType && newRight.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.cannotCreateBinaryExpressionFormat, newLeft.ToString(), newLeft.Type.Name, newRight.ToString(), newRight.Type.Name));

            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            ParameterExpression parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMethodCall(node);

            infoDictionary.Add(parameterExpression, this.TypeMappings);

            List<Expression> listOfArgumentsForNewMethod = node.Arguments.Aggregate(new List<Expression>(), (lst, next) =>
            {
                Expression mappedNext = ArgumentMapper.Create(this, next).MappedArgumentExpression;
                this.TypeMappings.AddTypeMapping(next.Type, mappedNext.Type);

                lst.Add(mappedNext);
                return lst;
            });//Arguments could be expressions or other objects. e.g. s => s.UserId  or a string "ZZZ".  For extention methods node.Arguments[0] is usually the helper object itself

            //type args are the generic type args e.g. T1 and T2 MethodName<T1, T2>(method arguments);
            List<Type> typeArgsForNewMethod = node.Method.IsGenericMethod
                ? node.Method.GetGenericArguments().Select(i => typeMappings.ContainsKey(i) ? typeMappings[i] : i).ToList()//not converting the type it is not in the typeMappings dictionary
                : null;

            MethodCallExpression resultExp = null;
            if (!node.Method.IsStatic)
            {
                Expression instance = ArgumentMapper.Create(this, node.Object).MappedArgumentExpression;

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
        #endregion Methods

        #region Private Methods
        protected string BuildFullName(List<PropertyMapInfo> propertyMapInfoList)
        {
            string fullName = string.Empty;
            foreach (PropertyMapInfo info in propertyMapInfoList)
            {
                if (info.CustomExpression != null)
                {
                    fullName = string.IsNullOrEmpty(fullName)
                        ? info.CustomExpression.GetMemberFullName()
                        : string.Concat(fullName, ".", info.CustomExpression.GetMemberFullName());
                }
                else
                {
                    StringBuilder additions = info.DestinationPropertyInfos.Aggregate(new StringBuilder(fullName), (sb, next) =>
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

        private void AddPropertyMapInfo(Type parentType, string name, List<PropertyMapInfo> propertyMapInfoList)
        {
            MemberInfo sourceMemberInfo = parentType.GetMember(name).First();
            MethodInfo methodInfo = null;
            PropertyInfo propertyInfo = null;
            FieldInfo fieldInfo = null;
            if ((methodInfo = sourceMemberInfo as MethodInfo) != null)
                propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> { methodInfo }));
            if ((propertyInfo = sourceMemberInfo as PropertyInfo) != null)
                propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> { propertyInfo }));
            if ((fieldInfo = sourceMemberInfo as FieldInfo) != null)
                propertyMapInfoList.Add(new PropertyMapInfo(null, new List<MemberInfo> { fieldInfo }));
        }

        protected void FindDestinationFullName(Type typeSource, Type typeDestination, string sourceFullName, List<PropertyMapInfo> propertyMapInfoList)
        {
            const string PERIOD = ".";
            if (typeSource == typeDestination)
            {
                string[] sourceFullNameArray = sourceFullName.Split(new char[] { PERIOD[0] }, StringSplitOptions.RemoveEmptyEntries);
                propertyMapInfoList = sourceFullNameArray.Aggregate(propertyMapInfoList, (list, next) =>
                {

                    if (list.Count == 0)
                    {
                        AddPropertyMapInfo(typeSource,  next, list);
                    }
                    else
                    {
                        PropertyMapInfo last = list[list.Count - 1];
                        AddPropertyMapInfo(last.CustomExpression == null
                            ? last.DestinationPropertyInfos[last.DestinationPropertyInfos.Count - 1].GetMemberType()
                            : last.CustomExpression.ReturnType, next, list);
                    }
                    return list;
                });
                return;
            }

            TypeMap typeMap = this.ConfigurationProvider.ResolveTypeMap(typeDestination, typeSource);//The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination 

            if (sourceFullName.IndexOf(PERIOD) < 0)
            {
                PropertyMap propertyMap = typeMap.GetPropertyMaps().SingleOrDefault(item => item.DestinationProperty.Name == sourceFullName);
                MemberInfo sourceMemberInfo = typeSource.GetMember(propertyMap.DestinationProperty.Name).First();
                if (propertyMap.ValueResolverConfig != null)
                {
                    #region Research
                    #endregion

                    throw new InvalidOperationException(Resource.customResolversNotSupported);
                }
                else if (propertyMap.CustomExpression != null)
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
                string propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(PERIOD));
                PropertyMap propertyMap = typeMap.GetPropertyMaps().SingleOrDefault(item => item.DestinationProperty.Name == propertyName);

                MemberInfo sourceMemberInfo = typeSource.GetMember(propertyMap.DestinationProperty.Name).First();
                if (propertyMap.CustomExpression == null && propertyMap.SourceMember == null)//If sourceFullName has a period then the SourceMember cannot be null.  The SourceMember is required to find the ProertyMap of its child object.
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, propertyName));

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomExpression, propertyMap.SourceMembers.ToList()));
                string childFullName = sourceFullName.Substring(sourceFullName.IndexOf(PERIOD) + 1);

                FindDestinationFullName(sourceMemberInfo.GetMemberType(), propertyMap.CustomExpression == null
                    ? propertyMap.SourceMember.GetMemberType()
                    : propertyMap.CustomExpression.ReturnType, childFullName, propertyMapInfoList);
            }
        }
        #endregion Private Methods
    }
}