namespace AutoMapper.Execution
{
    using System;
    using System.Reflection;

    public class PropertyNameResolver : IValueResolver
    {
        private readonly Type _sourceType;
        private readonly PropertyInfo _propertyInfo;

        public PropertyNameResolver(Type sourceType, string propertyName)
        {
            _sourceType = sourceType;
            _propertyInfo = sourceType.GetProperty(propertyName);
        }


        public object Resolve(object source, ResolutionContext context)
        {
            if (source == null)
                return null;

            var valueType = source.GetType();
            if (!(_sourceType.IsAssignableFrom(valueType)))
                throw new ArgumentException("Expected obj to be of type " + _sourceType + " but was " + valueType);

            var result = _propertyInfo.GetValue(source, null);

            return result;
        }
    }
}