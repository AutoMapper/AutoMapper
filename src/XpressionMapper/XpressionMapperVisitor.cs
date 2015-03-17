using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using XpressionMapper.ArgumentMappers;
using XpressionMapper.Extensions;
using XpressionMapper.Structures;

namespace XpressionMapper
{
    public class XpressionMapperVisitor : ExpressionVisitor
    {
        public XpressionMapperVisitor(Dictionary<Type, MapperInfo> infoDictionary)
        {
            this.infoDictionary = infoDictionary;
        }

        #region Variables
        private Dictionary<Type, MapperInfo> infoDictionary;
        #endregion Variables

        #region Properties
        public Dictionary<Type, MapperInfo> InfoDictionary
        {
            get { return this.infoDictionary; }
        }
        #endregion Properties

        #region Methods
        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            KeyValuePair<Type, MapperInfo> pair = infoDictionary.SingleOrDefault(a => a.Key == parameter.Type);
            if (!pair.Equals(default(KeyValuePair<Type, MapperInfo>)))
            {
                return pair.Value.NewParameter;
            }
            return base.VisitParameter(parameter);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath = null;

            Type sType = node.GetParameterType();
            if (sType != null && infoDictionary.ContainsKey(sType) && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            List<PropertyMapInfo> propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, infoDictionary[sType].DestType, sourcePath, propertyMapInfoList);
            string fullName = null;

            if (propertyMapInfoList[propertyMapInfoList.Count - 1].CustomExpression != null)
            {
                PropertyMapInfo last = propertyMapInfoList[propertyMapInfoList.Count - 1];
                propertyMapInfoList.Remove(last);

                //Get the fullname of the reference object - this means building the reference name from all but the last expression.
                fullName = BuildFullName(propertyMapInfoList);
                PrependParentNameVisitor visitor = new PrependParentNameVisitor(infoDictionary[sType].DestType, last.CustomExpression.Parameters[0].Type, fullName, infoDictionary[sType].NewParameter);
                Expression ex = visitor.Visit(last.CustomExpression.Body);
                return ex;
            }
            else
            {
                fullName = BuildFullName(propertyMapInfoList);
                MemberExpression me = infoDictionary[sType].NewParameter.BuildExpression(infoDictionary[sType].DestType, fullName);
                return me;
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var constantExpression = node.Right as ConstantExpression;
            if (constantExpression != null)
            {
                if (constantExpression.Value == null)
                    return base.VisitBinary(node.Update(node.Left, node.Conversion, Expression.Constant(null)));
            }

            constantExpression = node.Left as ConstantExpression;
            if (constantExpression != null)
            {
                if (constantExpression.Value == null)
                    return base.VisitBinary(node.Update(Expression.Constant(null), node.Conversion, node.Right));
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null)//not necessary to map instance methods - in fact mapping instance methods causes problems
                return base.VisitMethodCall(node);

            Type sType = node.GetParameterType();
            if (sType == null || !infoDictionary.ContainsKey(sType))
                return base.VisitMethodCall(node);

            //if (node.Object != null)//not necessary to map instance methods - in fact it mapping instance methods causes problems
                //return base.VisitMethodCall(node);

            List<Expression> listOfExpressionArgumentsForNewMethod = new List<Expression>();//e.g. s => s.UserId.  node.Arguments[0] is usually the helper object itself

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                ArgumentMapper argumentMapper = ArgumentMapper.Create(this, node.Arguments[i]);
                listOfExpressionArgumentsForNewMethod.Add(argumentMapper.MappedArgumentExpression);
            }

            //type args are the generic type args e.g. T1 and T2 MethodName<T1, T2>(method arguments);
            List<Type> typeArgsForNewMethod = node.Method.IsGenericMethod
                ? node.Method.GetGenericArguments().ToList().ConvertAll<Type>(i => infoDictionary.ContainsKey(i) ? infoDictionary[i].DestType : i)//not converting the type it is not in the info dictionary
                : null;

            MethodCallExpression resultExp = Expression.Call(node.Method.DeclaringType, node.Method.Name, typeArgsForNewMethod == null ? null : typeArgsForNewMethod.ToArray(), listOfExpressionArgumentsForNewMethod.ToArray());
            return resultExp;
        }
        #endregion Methods

        #region Private Methods
        private string BuildFullName(List<PropertyMapInfo> propertyMapInfoList)
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
                    fullName = string.IsNullOrEmpty(fullName)
                        ? info.DestinationPropertyInfo.Name
                        : string.Concat(fullName, ".", info.DestinationPropertyInfo.Name);
                }
            }

            return fullName;
        }

        private static void FindDestinationFullName(Type typeSource, Type typeDestination, string sourceFullName, List<PropertyMapInfo> propertyMapInfoList)
        {
            const string PERIOD = ".";
            TypeMap typeMap = Mapper.FindTypeMapFor(typeDestination, typeSource);//The destination becomes the source because to map a source expression to a destination expression,
            //we need the expressions used to create the source from the destination 

            if (sourceFullName.IndexOf(PERIOD) < 0)
            {
                PropertyMap propertyMap = typeMap.GetPropertyMaps().SingleOrDefault(item => item.DestinationProperty.Name == sourceFullName);
                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomExpression, propertyMap.SourceMember));
            }
            else
            {
                string propertyName = sourceFullName.Substring(0, sourceFullName.IndexOf(PERIOD));
                PropertyMap propertyMap = typeMap.GetPropertyMaps().SingleOrDefault(item => item.DestinationProperty.Name == propertyName);
                if (propertyMap.SourceMember == null)//If sourceFullName has a period then the SourceMember cannot be null.  The SourceMember is required to find the ProertyMap of its child object.
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.srcMemberCannotBeNullFormat, typeSource.Name, typeDestination.Name, propertyName));

                propertyMapInfoList.Add(new PropertyMapInfo(propertyMap.CustomExpression, propertyMap.SourceMember));
                string childFullName = sourceFullName.Substring(sourceFullName.IndexOf(PERIOD) + 1);
                FindDestinationFullName(typeSource.GetProperty(propertyMap.DestinationProperty.Name).PropertyType, ((PropertyInfo)propertyMap.SourceMember).PropertyType, childFullName, propertyMapInfoList);
            }
        }
        #endregion Private Methods
    }
}
