using System.Linq;
using System.Reflection;
using Machine.Specifications;

namespace AutoMapper.UnitTests.Tests
{
    [Subject(typeof(PropertyMap), "SourceMember property")]
    public abstract class PropertyMap_SourceMember_Specs
    {
        Establish context = () => Mapper.CreateMap<Source, Destination>();
        Cleanup after = () => Mapper.Reset();

        protected class Source
        {
            public int PropertyWithMatchingName { get; set; }
        }

        protected class Destination
        {
            public int PropertyWithMatchingName { get; set; }
        }
    }

    public class When_getting_the_SourceMember_of_a_PropertyMap_where_members_have_matching_names : PropertyMap_SourceMember_Specs
    {
        const string memberName = "PropertyWithMatchingName";
        static MemberInfo sourceMember;

        Because of = () => sourceMember =
            Mapper.FindTypeMapFor<Source, Destination>()
                .GetPropertyMaps()
                .Single(pm => pm.DestinationProperty.Name == memberName)
                .SourceMember;

        It should_not_be_null = () => sourceMember.ShouldNotBeNull();

        It should_have_the_matching_member_of_the_source_type_as_value = () =>
            sourceMember.ShouldBeTheSameAs(typeof(Source).GetProperty(memberName));
    }
}