using System;
using System.Linq;
using System.Reflection;
using Machine.Specifications;

namespace AutoMapper.UnitTests.Tests
{
    public abstract class PropertyMap_SourceMember_Specs
    {
        Establish context = () => Mapper.CreateMap<Source, Destination>()
            .ForMember(d => d.Ignored, o => o.Ignore())
            .ForMember(d => d.RenamedField, o => o.MapFrom(s => s.NamedProperty));

        Cleanup after = () => Mapper.Reset();

        protected class Source
        {
            public int PropertyWithMatchingName { get; set; }
            public NestedSource NestedSource { get; set; }
            public int NamedProperty { get; set; }
        }

        protected class NestedSource
        {
            public int SomeField;
        }

        protected class Destination
        {
            public int PropertyWithMatchingName { get; set; }
            public string NestedSourceSomeField;
            public string Ignored { get; set; }
            public string RenamedField;
        }
    }

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_members_have_matching_names : PropertyMap_SourceMember_Specs
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

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_the_destination_member_is_flattened : PropertyMap_SourceMember_Specs
    {
        static MemberInfo sourceMember;

        Because of = () => sourceMember =
            Mapper.FindTypeMapFor<Source, Destination>()
                .GetPropertyMaps()
                .Single(pm => pm.DestinationProperty.Name == "NestedSourceSomeField")
                .SourceMember;

        It should_not_be_null = () => sourceMember.ShouldNotBeNull();

        It should_have_the_member_of_the_nested_source_type_as_value = () =>
            sourceMember.ShouldBeTheSameAs(typeof(NestedSource).GetField("SomeField"));
    }

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_the_destination_member_is_ignored : PropertyMap_SourceMember_Specs
    {
        static Exception exception;
        static MemberInfo sourceMember;

        Because of = () => exception = Catch.Exception(() =>
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "Ignored")
                    .SourceMember
        );

        It should_not_throw_an_exception = () => exception.ShouldBeNull();

        It should_be_null = () => sourceMember.ShouldBeNull();
    }

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_the_destination_member_is_projected : PropertyMap_SourceMember_Specs
    {
        static MemberInfo sourceMember;

        Because of = () => sourceMember =
            Mapper.FindTypeMapFor<Source, Destination>()
                .GetPropertyMaps()
                .Single(pm => pm.DestinationProperty.Name == "RenamedField")
                .SourceMember;

        It should_not_be_null = () => sourceMember.ShouldNotBeNull();

        It should_have_the_projected_member_of_the_source_type_as_value = () =>
            sourceMember.ShouldBeTheSameAs(typeof(Source).GetProperty("NamedProperty"));
    }
}