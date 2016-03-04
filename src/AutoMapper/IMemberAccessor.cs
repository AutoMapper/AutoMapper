namespace AutoMapper
{
    public interface IMemberAccessor : IMemberGetter
    {
        void SetValue(object destination, object value);
    }

    public class MemberAccessor : MemberGetter, IMemberAccessor
    {
        public void SetValue(object destination, object value)
        {
            
        }
    }
}