using System;
using FastExpressionCompiler;
using static System.Linq.Expressions.Expression;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class FastExpressionCompilerBug
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        [Fact]
        public void ShouldWork()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());
            var mapper = config.CreateMapper();
            var expression = mapper.ConfigurationProvider.BuildExecutionPlan(typeof(Source), typeof(Dest));

            var source = new Source { Value = 5 };
            var dest = mapper.Map<Dest>(source);

            dest.Value.ShouldBe(source.Value);
        }

        public class DefaultEnumValueToString
        {
            class Source
            {
                public ConsoleColor Color { get; set; }
            }

            class Destination
            {
                public string Color { get; set; }
            }

            [Fact]
            public void Should_map_ok()
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());
                var mapper = config.CreateMapper();
                var expression = mapper.ConfigurationProvider.BuildExecutionPlan(typeof(Source), typeof(Destination));
                var expr2 = config.BuildExecutionPlan(typeof(ConsoleColor), typeof(string));

                mapper.Map<ConsoleColor, string>(default);

                var source = new Source();
                var dest = mapper.Map<Source, Destination>(source);
                dest.Color.ShouldBe("Black");
            }

            [Fact]
            public void ShouldAlsoWork()
            {
                var srcParam = Parameter(typeof(Source), "src");
                var destParam = Parameter(typeof(Destination), "dest");
                var resolvedValueParam = Parameter(typeof(ConsoleColor), "resolvedValue");
                var propertyValueParam = Parameter(typeof(string), "propertyValue");

                var expression = Lambda<Func<Source, Destination>>(
                    Block(typeof(Destination), new[] {destParam, propertyValueParam},
                        Assign(destParam, New(typeof(Destination).GetConstructors()[0])),
                        Assign(propertyValueParam, Call(Property(srcParam, "Color"), "ToString", new Type[0])),
                        Assign(Property(destParam, "Color"), propertyValueParam),
                        destParam
                    ),
                    srcParam
                );

                var compiled = expression.CompileFast(true);

                var src = new Source();
                var dest = compiled(src);

                dest.ShouldNotBeNull();
                dest.Color.ShouldBe("Black");
            }

        }

        [Fact]
        public void SimpleExample()
        {
            var expression = Lambda<Func<int>>(
                TryCatch(
                    Constant(0),
                    Catch(typeof(Exception),
                        Block(
                            Throw(Constant(new Exception("Explode"))),
                            Constant(default(int))
                        )
                    )
                )
            );
            var builtIn = expression.Compile()();
            builtIn.ShouldBe(0);
            var compileFast = expression.CompileFast()();
            compileFast.ShouldBe(0);
        }

        [Fact]
        public void ShouldManuallyWork()
        {
            var srcParam = Parameter(typeof(Source), "src");
            var typeMapDestParam = Parameter(typeof(Dest), "typeMapDest");
            var destParam = Parameter(typeof(Dest), "dest");
            var resolvedValueParam = Parameter(typeof(int), "resolvedValue");
            var exceptionParameter = Parameter(typeof(Exception), "ex");
            var constructorInfo = typeof(AutoMapperMappingException).GetConstructor(new Type[] { typeof(string), typeof(Exception), typeof(TypePair), typeof(TypeMap), typeof(PropertyMap) });
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());
            var typeMap = config.FindTypeMapFor<Source, Dest>();
            var memberMap = typeMap.FindOrCreatePropertyMapFor(typeof(Dest).GetProperty("Value"));

            var expression = Lambda<Func<Source, Dest, ResolutionContext, Dest>>(
                Block(
                    Condition(
                        Equal(srcParam, Constant(null)),
                        Default(typeof(Dest)),
                        Block(typeof(Dest), new[] { typeMapDestParam },
                            Assign(typeMapDestParam, Coalesce(destParam, New(typeof(Dest).GetConstructors()[0]))),
                            TryCatch(
                                /* Assign src.Value */
                                Block(typeof(void), new[] { resolvedValueParam },
                                    Block(
                                        Assign(resolvedValueParam,
                                            Condition(Or(Equal(srcParam, Constant(null)), Constant(false)),
                                                Default(typeof(int)),
                                                Property(srcParam, "Value"))
                                        ),
                                        Assign(Property(typeMapDestParam, "Value"), resolvedValueParam)
                                    )
                                ),
                                Catch(exceptionParameter,
                                    Throw(New(constructorInfo,
                                        Constant("Error mapping types."),
                                        exceptionParameter,
                                        Constant(typeMap.Types),
                                        Constant(typeMap),
                                        Constant(memberMap)
                                        )
                                    )
                                )
                            ),
                            typeMapDestParam
                        )
                    )
                ),
                srcParam,
                destParam,
                Parameter(typeof(ResolutionContext), "ctxt")
            );

            var compiled = expression.CompileFast();

            var src = new Source { Value = 5 };
            var dest = compiled(src, null, null);

            dest.ShouldNotBeNull();
        }

    }

    public class AnotherFastExpressionCompilerBug
    {
        public enum Status
        {
            InProgress = 1,
            Complete = 2
        }

        public class OrderWithNullableStatus
        {
            public Status? Status { get; set; }
        }

        public class OrderDtoWithNullableStatus
        {
            public Status? Status { get; set; }
        }

        [Fact]
        public void ShouldAlsoWork()
        {
            var srcParam = Parameter(typeof(OrderWithNullableStatus), "src");
            var destParam = Parameter(typeof(OrderDtoWithNullableStatus), "dest");
            var resolvedValueParam = Parameter(typeof(Status?), "resolvedValue");
            var propertyValueParam = Parameter(typeof(Status?), "propertyValue");

            var expression = Lambda<Func<OrderWithNullableStatus, OrderDtoWithNullableStatus>>(
                Block(typeof(OrderDtoWithNullableStatus), new[] { destParam, resolvedValueParam, propertyValueParam },
                    Assign(destParam, New(typeof(OrderDtoWithNullableStatus).GetConstructors()[0])),
                    Assign(resolvedValueParam, Property(srcParam, "Status")),
                    Assign(propertyValueParam, Condition(
                        Equal(resolvedValueParam, Constant(null)),
                        Default(typeof(Status?)),
                        Convert(Property(resolvedValueParam, "Value"), typeof(Status?)))),
                    Assign(Property(destParam, "Status"), propertyValueParam),
                    destParam
                ),
                srcParam
            );

            var compiled = expression.CompileFast(true);

            var src = new OrderWithNullableStatus
            {
                Status = Status.InProgress
            };

            var dest = compiled(src);

            dest.ShouldNotBeNull();
            dest.Status.ShouldBe(Status.InProgress);
        }
    }
}