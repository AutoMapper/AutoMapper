namespace AutoMapper.Internal
{
    using System;

    public interface IEnumNameValueMapper
    {
        bool IsMatch(Type enumDestinationType, string sourceValue);
        object Convert(Type enumSourceType, Type enumDestinationType, ResolutionContext context);
    }
}