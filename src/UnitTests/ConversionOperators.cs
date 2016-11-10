using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    namespace ConversionOperators
    {
        public class When_mapping_to_classes_with_implicit_conversion_operators_on_the_destination
        {
            private Bar _bar;

            public class Foo
            {
                public string Value { get; set; }
            }

            public class Bar
            {
                public string OtherValue { get; set; }

                public static implicit operator Bar(Foo other)
                {
                    return new Bar
                    {
                        OtherValue = other.Value
                    };
                }

            }

            [Fact]
            public void Should_use_the_implicit_conversion_operator()
            {
                var source = new Foo { Value = "Hello" };
                var config = new MapperConfiguration(cfg => { });

                _bar = config.CreateMapper().Map<Foo, Bar>(source);

                _bar.OtherValue.ShouldEqual("Hello");
            }
        }
        
        public class When_mapping_to_classes_with_implicit_conversion_operators_on_the_source
        {
            private Bar _bar;

            public class Foo
            {
                public string Value { get; set; }

                public static implicit operator Bar(Foo other)
                {
                    return new Bar
                    {
                        OtherValue = other.Value
                    };
                }

                public static implicit operator string(Foo other)
                {
                    return other.Value;
                }

            }

            public class Bar
            {
                public string OtherValue { get; set; }
            }

            [Fact]
            public void Should_use_the_implicit_conversion_operator()
            {
                var source = new Foo { Value = "Hello" };

                var config = new MapperConfiguration(cfg => { });
                _bar = config.CreateMapper().Map<Foo, Bar>(source);

                _bar.OtherValue.ShouldEqual("Hello");
            }
        }

        public class When_mapping_to_classes_with_explicit_conversion_operator_on_the_destination
        {
            private Bar _bar;

            public class Foo
            {
                public string Value { get; set; }
            }

            public class Bar
            {
                public string OtherValue { get; set; }

                public static explicit operator Bar(Foo other)
                {
                    return new Bar
                    {
                        OtherValue = other.Value
                    };
                }
            }

            [Fact]
            public void Should_use_the_explicit_conversion_operator()
            {
                var config = new MapperConfiguration(cfg => { });
                _bar = config.CreateMapper().Map<Foo, Bar>(new Foo { Value = "Hello" });
                _bar.OtherValue.ShouldEqual("Hello");
            }
        }

        public class When_mapping_to_classes_with_explicit_conversion_operator_on_the_source
        {
            private Bar _bar;

            public class Foo
            {
                public string Value { get; set; }

                public static explicit operator Bar(Foo other)
                {
                    return new Bar
                    {
                        OtherValue = other.Value
                    };
                }
            }

            public class Bar
            {
                public string OtherValue { get; set; }
            }

            [Fact]
            public void Should_use_the_explicit_conversion_operator()
            {
                var config = new MapperConfiguration(cfg => { });
                _bar = config.CreateMapper().Map<Foo, Bar>(new Foo { Value = "Hello" });
                _bar.OtherValue.ShouldEqual("Hello");
            }
        }
    }
}