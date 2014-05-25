using System;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    internal class EquivilentExpression<TSource,TDestination> : IEquivilentExpression 
        where TSource : class 
        where TDestination : class
    {
        private readonly Expression<Func<TSource, TDestination, bool>> _equivilentExpression;

        public EquivilentExpression(Expression<Func<TSource,TDestination,bool>> equivilentExpression)
        {
            _equivilentExpression = equivilentExpression;
        }

        public bool IsEquivlent(object source, object destination)
        {
            if(!(source is TSource))
                throw new EquivilentExpressionNotOfTypeException(source.GetType(), typeof(TSource));
            if (!(destination is TDestination))
                throw new EquivilentExpressionNotOfTypeException(destination.GetType(), typeof(TDestination));
            return _equivilentExpression.Compile()(source as TSource, destination as TDestination);
        }

        private class EquivilentExpressionNotOfTypeException : Exception
        {
            private readonly Type _objectType;
            private readonly Type _expectedType;

            public EquivilentExpressionNotOfTypeException(Type objectType, Type expectedType)
            {
                _objectType = objectType;
                _expectedType = expectedType;
            }

            public override string Message
            {
                get { return string.Format("{0} does not equal or inherit from {1}", _objectType.Name, _expectedType.Name); }
            }
        }
    }
}