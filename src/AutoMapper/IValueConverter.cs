namespace AutoMapper
{
    public interface IValueConverter<in TSourceMember, out TDestinationMember>
    {
        TDestinationMember Convert(TSourceMember sourceMember, ResolutionContext context);
    }
}