namespace AutoMapper.Configuration.Conventions
{
    public class MemberNameReplacer
    {
        public MemberNameReplacer(string originalValue, string newValue)
        {
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        public string OriginalValue { get; }
        public string NewValue { get; }
    }
}