using System;
using System.Linq;
using System.Reflection;
using Machine.Specifications;

namespace AutoMapper.UnitTests.Tests
{
    public abstract class PropertyMap_SourceMember_Specs
    {
        Cleanup after = () => Mapper.Reset();
    }

    public abstract class using_generic_configuration : PropertyMap_SourceMember_Specs
    {
        Establish context = () => Mapper.CreateMap<Source, Destination>()
            .ForMember(d => d.Ignored, o => o.Ignore())
            .ForMember(d => d.RenamedField, o => o.MapFrom(s => s.NamedProperty))
            .ForMember(d => d.IntField, o => o.ResolveUsing<FakeResolver>().FromMember(s => s.StringField));

        protected class Source
        {
            public int PropertyWithMatchingName { get; set; }
            public NestedSource NestedSource { get; set; }
            public int NamedProperty { get; set; }
            public string StringField;
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
            public int IntField;
        }

        class FakeResolver : ValueResolver<string, int>
        {
            protected override int ResolveCore(string source)
            {
                return default(int);
            }
        }
    }

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_members_have_matching_names : using_generic_configuration
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
    public class when_the_destination_member_is_flattened : using_generic_configuration
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
    public class when_the_destination_member_is_ignored : using_generic_configuration
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
    public class when_the_destination_member_is_projected : using_generic_configuration
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

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_the_destination_member_is_resolved_from_a_source_member : using_generic_configuration
    {
        static MemberInfo sourceMember;

        Because of = () => sourceMember =
            Mapper.FindTypeMapFor<Source, Destination>()
                .GetPropertyMaps()
                .Single(pm => pm.DestinationProperty.Name == "IntField")
                .SourceMember;

        It should_not_be_null = () => sourceMember.ShouldNotBeNull();

        It should_have_the_member_of_the_source_type_it_is_resolved_by_as_value = () =>
            sourceMember.ShouldBeTheSameAs(typeof(Source).GetField("StringField"));
    }

    public abstract class using_nongeneric_configuration : PropertyMap_SourceMember_Specs
    {
        Establish context = () => Mapper.CreateMap(typeof(Source), typeof(Destination))
            .ForMember("RenamedProperty", o => o.MapFrom("NamedProperty"));

        protected class Source
        {
            public int NamedProperty { get; set; }
        }

        protected class Destination
        {
            public string RenamedProperty { get; set; }
        }
    }

    [Subject(typeof(PropertyMap), ".SourceMember")]
    public class when_the_destination_property_is_projected : using_nongeneric_configuration
    {
        static MemberInfo sourceMember;

        Because of = () => sourceMember =
            Mapper.FindTypeMapFor<Source, Destination>()
                .GetPropertyMaps()
                .Single(pm => pm.DestinationProperty.Name == "RenamedProperty")
                .SourceMember;

        It should_not_be_null = () => sourceMember.ShouldNotBeNull();

        It should_have_the_projected_member_of_the_source_type_as_value = () =>
            sourceMember.ShouldBeTheSameAs(typeof(Source).GetProperty("NamedProperty"));
    }
}