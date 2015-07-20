namespace AutoMapper.Internal
{
    public class MemberNameReplacer
    {
        public MemberNameReplacer(string originalValue, string newValue)
        {
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        public string OriginalValue { get; private set; }
        public string NewValue { get; private set; }
    }
}