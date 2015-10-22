namespace AutoMapper
{
    public interface IMemberAccessor : IMemberGetter
    {
        void SetValue(object destination, object value);
    }
}