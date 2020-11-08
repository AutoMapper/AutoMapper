using System;

namespace AutoMapper.Internal
{
    public static class ElementTypeHelper
    {
        public static Type GetElementType(Type enumerableType) => enumerableType.IsArray ?
            enumerableType.GetElementType() :
            enumerableType.GetIEnumerableType()?.GenericTypeArguments[0] ?? typeof(object);
        public static Type[] GetElementTypes(Type enumerableType)
        {
            var iDictionaryType = enumerableType.GetDictionaryType();
            if (iDictionaryType != null)
            {
                return iDictionaryType.GenericTypeArguments;
            }
            var iReadOnlyDictionaryType = enumerableType.GetReadOnlyDictionaryType();
            if (iReadOnlyDictionaryType != null)
            {
                return iReadOnlyDictionaryType.GenericTypeArguments;
            }
            return new[] { GetElementType(enumerableType) };
        }
    }
}