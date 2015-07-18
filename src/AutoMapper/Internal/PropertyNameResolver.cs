namespace AutoMapper.Internal
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


        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value == null)
                return source;

            var valueType = source.Value.GetType();
            if (!(_sourceType.IsAssignableFrom(valueType)))
                throw new ArgumentException("Expected obj to be of type " + _sourceType + " but was " + valueType);

            var result = _propertyInfo.GetValue(source.Value, null);

            return source.New(result);
        }
    }
}