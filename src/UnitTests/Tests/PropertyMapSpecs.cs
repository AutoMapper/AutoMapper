using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Should;

namespace AutoMapper.UnitTests.Tests
{
    public abstract class using_generic_configuration : AutoMapperSpecBase
    {
        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Destination>()
                .ForMember(d => d.Ignored, o => o.Ignore())
                .ForMember(d => d.RenamedField, o => o.MapFrom(s => s.NamedProperty))
                .ForMember(d => d.IntField, o => o.ResolveUsing<FakeResolver>().FromMember(s => s.StringField))
                .ForMember("IntProperty", o => o.ResolveUsing<FakeResolver>().FromMember("AnotherStringField"))
                .ForMember(d => d.IntProperty3,
                           o => o.ResolveUsing(typeof (FakeResolver)).FromMember(s => s.StringField3))
                .ForMember(d => d.IntField4, o => o.ResolveUsing(new FakeResolver()).FromMember("StringField4"));
        }

        protected class Source
        {
            public int PropertyWithMatchingName { get; set; }
            public NestedSource NestedSource { get; set; }
            public int NamedProperty { get; set; }
            public string StringField;
            public string AnotherStringField;
            public string StringField3;
            public string StringField4;
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
            public int IntProperty { get; set; }
            public int IntProperty3 { get; set; }
            public int IntField4;
        }

        class FakeResolver : ValueResolver<string, int>
        {
            protected override int ResolveCore(string source)
            {
                return default(int);
            }
        }
    }

    public class when_members_have_matching_names : using_generic_configuration
    {
        const string memberName = "PropertyWithMatchingName";
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == memberName)
                    .SourceMember;
        }

        [Fact]
        public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact]
        public void should_have_the_matching_member_of_the_source_type_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetProperty(memberName));
        }
    }

    public class when_the_destination_member_is_flattened : using_generic_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "NestedSourceSomeField")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact] public void should_have_the_member_of_the_nested_source_type_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (NestedSource).GetField("SomeField"));
        }
    }

    public class when_the_destination_member_is_ignored : using_generic_configuration
    {
        static Exception exception;
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            try
            {
                sourceMember =
                    Mapper.FindTypeMapFor<Source, Destination>()
                        .GetPropertyMaps()
                        .Single(pm => pm.DestinationProperty.Name == "Ignored")
                        .SourceMember;

            }
            catch (Exception ex)
            {
                exception = ex;
            }
                                        
        }

        [Fact] public void should_not_throw_an_exception()
        {
            exception.ShouldBeNull();
        }

        [Fact] public void should_be_null()
        {
            sourceMember.ShouldBeNull();
        }
    }

    public class when_the_destination_member_is_projected : using_generic_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "RenamedField")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact] public void should_have_the_projected_member_of_the_source_type_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetProperty("NamedProperty"));
        }
    }

    public class when_the_destination_member_is_resolved_from_a_source_member : using_generic_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "IntField")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact] public void should_have_the_member_of_the_source_type_it_is_resolved_from_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetField("StringField"));
        }
    }

    public class when_the_destination_property_is_resolved_from_a_source_member_using_the_Magic_String_overload : using_generic_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "IntProperty")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact] public void should_have_the_member_of_the_source_type_it_is_resolved_from_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetField("AnotherStringField"));
        }
    }

    public class when_the_destination_property_is_resolved_from_a_source_member_using_the_non_generic_resolve_method : using_generic_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "IntProperty3")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact] public void should_have_the_member_of_the_source_type_it_is_resolved_from_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetField("StringField3"));
        }
    }

    public class when_the_destination_property_is_resolved_from_a_source_member_using_non_the_generic_resolve_method_and_the_Magic_String_overload : using_generic_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "IntField4")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact] public void should_have_the_member_of_the_source_type_it_is_resolved_from_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetField("StringField4"));
        }
    }

    public abstract class using_nongeneric_configuration : AutoMapperSpecBase
    {
        protected override void Establish_context()
        {
            Mapper.CreateMap(typeof (Source), typeof (Destination))
                .ForMember("RenamedProperty", o => o.MapFrom("NamedProperty"));
        }

        protected class Source
        {
            public int NamedProperty { get; set; }
        }

        protected class Destination
        {
            public string RenamedProperty { get; set; }
        }
    }

    public class when_the_destination_property_is_projected : using_nongeneric_configuration
    {
        static MemberInfo sourceMember;

        protected override void Because_of()
        {
            sourceMember =
                Mapper.FindTypeMapFor<Source, Destination>()
                    .GetPropertyMaps()
                    .Single(pm => pm.DestinationProperty.Name == "RenamedProperty")
                    .SourceMember;
        }

        [Fact] public void should_not_be_null()
        {
            sourceMember.ShouldNotBeNull();
        }

        [Fact]
        public void should_have_the_projected_member_of_the_source_type_as_value()
        {
            sourceMember.ShouldBeSameAs(typeof (Source).GetProperty("NamedProperty"));
        }
    }
}