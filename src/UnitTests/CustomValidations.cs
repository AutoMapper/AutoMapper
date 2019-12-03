using System;
using System.Linq.Expressions;
using AutoMapper.Mappers;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class CustomValidations
    {
        public class Source
        {
        }

        public class Destination
        {
        }

        public class When_using_custom_validation
        {
            bool _calledForRoot = false;
            bool _calledForValues = false;
            bool _calledForInt = false;

            public class Source
            {
                public int[] Values { get; set; }
            }

            public class Dest
            {
                public int[] Values { get; set; }
            }

            [Fact]
            public void Should_call_the_validator()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.Advanced.Validator(Validator);
                    cfg.CreateMap<Source, Dest>();
                });

                config.AssertConfigurationIsValid();

                _calledForRoot.ShouldBeTrue();
                _calledForValues.ShouldBeTrue();
                _calledForInt.ShouldBeTrue();
            }

            private void Validator(ValidationContext context)
            {
                if (context.TypeMap != null)
                {
                    _calledForRoot = true;
                    context.TypeMap.Types.ShouldBe(context.Types);
                    context.Types.SourceType.ShouldBe(typeof(Source));
                    context.Types.DestinationType.ShouldBe(typeof(Dest));
                    context.ObjectMapper.ShouldBeNull();
                    context.MemberMap.ShouldBeNull();
                }
                else
                {
                    context.MemberMap.SourceMember.Name.ShouldBe("Values");
                    context.MemberMap.DestinationName.ShouldBe("Values");
                    if (context.Types.Equals(new TypePair(typeof(int), typeof(int))))
                    {
                        _calledForInt = true;
                        context.ObjectMapper.ShouldBeOfType<AssignableMapper>();
                    }
                    else
                    {
                        _calledForValues = true;
                        context.ObjectMapper.ShouldBeOfType<ArrayCopyMapper>();
                        context.Types.SourceType.ShouldBe(typeof(int[]));
                        context.Types.DestinationType.ShouldBe(typeof(int[]));
                    }
                }
            }
        }

        public class When_using_custom_validation_for_convertusing_with_mappingfunction : NonValidatingSpecBase
        {
            // Nullable so can see a false state
            private static bool? _validated;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                Func<Source, Destination, Destination> mappingFunction = (source, destination) => new Destination();
                cfg.CreateMap<Source, Destination>().ConvertUsing(mappingFunction);
                cfg.Advanced.Validator(SetValidated);
            });

            private static void SetValidated(ValidationContext context)
            {
                if (context.TypeMap.SourceType == typeof(Source) &&
                    context.TypeMap.DestinationTypeToUse == typeof(Destination))
                {
                    _validated = true;
                }
            }

            [Fact]
            public void Validator_should_be_called_by_AssertConfigurationIsValid()
            {
                _validated.ShouldBeNull();

                Configuration.AssertConfigurationIsValid();

                _validated.ShouldBe(true);
            }
        }

        public class When_using_custom_validation_for_convertusing_with_typeconvertertype : NonValidatingSpecBase
        {
            // Nullable so can see a false state
            private static bool? _validated;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>().ConvertUsing<CustomTypeConverter>();
                cfg.Advanced.Validator(SetValidated);
            });

            private static void SetValidated(ValidationContext context)
            {
                if (context.TypeMap.SourceType == typeof(Source) &&
                    context.TypeMap.DestinationTypeToUse == typeof(Destination))
                {
                    _validated = true;
                }
            }

            [Fact]
            public void Validator_should_be_called_by_AssertConfigurationIsValid()
            {
                _validated.ShouldBeNull();

                Configuration.AssertConfigurationIsValid();

                _validated.ShouldBe(true);
            }

            internal class CustomTypeConverter : ITypeConverter<Source, Destination>
            {
                public Destination Convert(Source source, Destination destination, ResolutionContext context)
                {
                    return new Destination();
                }
            }
        }

        public class When_using_custom_validation_for_convertusing_with_typeconverter_instance : NonValidatingSpecBase
        {
            // Nullable so can see a false state
            private static bool? _validated;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>().ConvertUsing(new CustomTypeConverter());
                cfg.Advanced.Validator(SetValidated);
            });

            private static void SetValidated(ValidationContext context)
            {
                if (context.TypeMap.SourceType == typeof(Source) &&
                    context.TypeMap.DestinationTypeToUse == typeof(Destination))
                {
                    _validated = true;
                }
            }

            [Fact]
            public void Validator_should_be_called_by_AssertConfigurationIsValid()
            {
                _validated.ShouldBeNull();

                Configuration.AssertConfigurationIsValid();

                _validated.ShouldBe(true);
            }

            internal class CustomTypeConverter : ITypeConverter<Source, Destination>
            {
                public Destination Convert(Source source, Destination destination, ResolutionContext context)
                {
                    return new Destination();
                }
            }
        }

        public class When_using_custom_validation_for_convertusing_with_mappingexpression : NonValidatingSpecBase
        {
            // Nullable so can see a false state
            private static bool? _validated;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                Expression<Func<Source, Destination>> mappingExpression = source => new Destination();
                cfg.CreateMap<Source, Destination>().ConvertUsing(mappingExpression);
                cfg.Advanced.Validator(SetValidated);
            });

            private static void SetValidated(ValidationContext context)
            {
                if (context.TypeMap.SourceType == typeof(Source) &&
                    context.TypeMap.DestinationTypeToUse == typeof(Destination))
                {
                    _validated = true;
                }
            }

            [Fact]
            public void Validator_should_be_called_by_AssertConfigurationIsValid()
            {
                _validated.ShouldBeNull();

                Configuration.AssertConfigurationIsValid();

                _validated.ShouldBe(true);
            }
        }
    }
}