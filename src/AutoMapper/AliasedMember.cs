namespace AutoMapper
{
    public class AliasedMember
    {
        public AliasedMember(string member, string alias)
        {
            Member = member;
            Alias = alias;
        }

        public string Member { get; }
        public string Alias { get; }

        public bool Equals(AliasedMember other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Member, Member) && Equals(other.Alias, Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (AliasedMember)) return false;
            return Equals((AliasedMember) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Member.GetHashCode()*397) ^ Alias.GetHashCode();
            }
        }
    }
}