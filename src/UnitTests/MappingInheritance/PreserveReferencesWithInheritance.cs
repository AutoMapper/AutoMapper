namespace AutoMapper.UnitTests
{
    namespace Source
    {
        public class Instance
        {
            public Type Type { get; set; }
            public Instance Definition { get; set; }
        }

        public sealed class Class : Instance
        {
            private IList<Member> _properties;
            public IList<Member> Properties
            {
                get { return _properties ?? (_properties = new List<Member>()); }
                set { _properties = value; }
            }
        }

        public abstract class Member : Instance
        {
            public string Name { get; set; }
        }

        public sealed class Parameter : Member
        {
            public int Position { get; set; }
        }

        public sealed class Property : Member
        {
        }

        public sealed class Field : Member
        {
        }
    }

    namespace Target
    {
        public class Instance
        {
            public Type Type { get; set; }
            public Instance Definition { get; set; }
        }

        public sealed class Class : Instance
        {
            public IList<Member> Properties { get; set; }
        }

        public abstract class Member : Instance
        {
            public string Name { get; set; }
        }

        public sealed class Parameter : Member
        {
            public int Position { get; set; }
        }

        public sealed class Property : Member
        {
        }

        public sealed class Field : Member
        {
        }
    }

    public class PreserveReferencesWithInheritance : AutoMapperSpecBase
    {
        List<Target.Member> _destination;

        protected override MapperConfiguration CreateConfiguration() => new(cfg=>
        {
            cfg.CreateMap<Source.Instance, Target.Instance>()
              .Include<Source.Class, Target.Class>()
              .Include<Source.Member, Target.Member>();
            cfg.CreateMap<Source.Member, Target.Member>()
                .Include<Source.Property, Target.Property>()
                .Include<Source.Parameter, Target.Parameter>()
                .Include<Source.Field, Target.Field>();
            cfg.CreateMap<Source.Class, Target.Class>();
            cfg.CreateMap<Source.Property, Target.Property>();
            cfg.CreateMap<Source.Parameter, Target.Parameter>();
            cfg.CreateMap<Source.Field, Target.Field>();
        });
        [Fact]
        public void Should_work()
        {
            var field = new Source.Field { Name = "AddResult", Type = typeof(Int32) };
            var @class = new Source.Class { Properties = new List<Source.Member> { field }, Type = typeof(float) };
            var returnValue = new Source.Instance { Type = typeof(float), Definition = @class };
            @class.Definition = @class;

            var source = new List<Source.Member> { new Source.Property { Name = "(return)", Definition = returnValue.Definition, Type = returnValue.Type }};
            _destination = Mapper.Map<List<Target.Member>>(source);
        }
    }
}