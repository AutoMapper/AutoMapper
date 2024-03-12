namespace AutoMapper.UnitTests
{
    namespace AssemblyScanning
    {
        public class When_scanning_by_assembly : NonValidatingSpecBase
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(new[] { typeof(When_scanning_by_assembly).Assembly, typeof(Mapper).Assembly });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Count.ShouldBeGreaterThan(0);
            }

            [Fact]
            public void Should_load_internal_profiles() => GetProfiles().Where(t => t.Name == InternalProfile.Name).ShouldNotBeEmpty();
        }

        internal class InternalProfile : Profile
        {
            public const string Name = "InternalProfile";

            public InternalProfile() : base(Name)
            {
            }
        }

        public class When_scanning_by_type : NonValidatingSpecBase
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(new[] { typeof(When_scanning_by_assembly) });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Count.ShouldBeGreaterThan(0);
            }
        }

        public class When_scanning_by_name : NonValidatingSpecBase
        {
            private static readonly Assembly AutoMapperAssembly = typeof(When_scanning_by_name).Assembly;

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                cfg.AddMaps(new[] { AutoMapperAssembly.FullName });
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            });

            private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) => args.Name == AutoMapperAssembly.FullName ? AutoMapperAssembly : null;

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Count.ShouldBeGreaterThan(0);
            }
        }
        
        public class When_scanning_by_assembly_specifying_map_with_nested_profile_class : NonValidatingSpecBase
        {
            public class Source {
                public int Value { get; set; }
            }
            
            public class Dest {
                public int Value { get; set; }
        
                private class Mapping : Profile
                {
                    public Mapping() {
                        CreateMap<Source, Dest>();
                    }
                }
            }
        
            protected override MapperConfiguration CreateConfiguration() => new(cfg => {
                cfg.AddMaps(typeof(When_scanning_by_assembly_specifying_map_with_nested_profile_class).Assembly);
            });
        
            [Fact]
            public void Should_map() {
                var source = new Source { Value = 5 };
                var dest = Mapper.Map<Dest>(source);
        
                dest.Value.ShouldBe(5);
            }
        
            [Fact]
            public void Should_validate_successfully() {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid<Source, Dest>);
            }
        }

        public class When_scanning_by_assembly_with_nested_closed_generic_destination : NonValidatingSpecBase
        {
            public class Source {
                public int Value { get; set; }
            }
            
            public class Dest<T> {
                public T Value { get; set; }
        
                private class GenericMapping : Profile
                {
                    public GenericMapping() {
                        CreateMap<Source, Dest<int>>();
                    }
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg => {
                cfg.AddMaps(typeof(When_scanning_by_assembly_with_nested_closed_generic_destination).Assembly);
            });

            [Fact]
            public void Should_map() {
                var source = new Source { Value = 5 };
                var dest = Mapper.Map<Dest<int>>(source);

                dest.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully() {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid<Source, Dest<int>>);
            }
        }
        
        public class When_scanning_by_assembly_with_nested_class_and_open_generic_destination : NonValidatingSpecBase
        {
            public class Source<T> {
                public T Value { get; set; }
            }
            
            public class Dest<T> {
                public T Value { get; set; }
        
                private class GenericMapping : Profile
                {
                    public GenericMapping()
                    {
                        CreateMap(typeof(Source<>), typeof(Dest<>));
                    }
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg => {
                cfg.AddMaps(typeof(When_scanning_by_assembly_with_nested_class_and_open_generic_destination).Assembly);
            });

            [Fact]
            public void Should_map_int() {
                var source = new Source<int> { Value = 5 };
                var dest = Mapper.Map<Dest<int>>(source);

                dest.Value.ShouldBe(5);
            }
            
            [Fact]
            public void Should_map_string() {
                var source = new Source<string> { Value = "foo" };
                var dest = Mapper.Map<Dest<string>>(source);

                dest.Value.ShouldBe("foo");
            }
            
            [Fact]
            public void Should_map_object() {
                var source = new Source<object> { Value = new object() };
                var dest = Mapper.Map<Dest<object>>(source);

                dest.Value.ShouldBe(source.Value);
            }
            
            [Fact]
            public void Should_map_class () {
                var source = new Source<Source<int>> { Value = new Source<int> { Value = 5 } };
                var dest = Mapper.Map<Dest<Source<int>>>(source);

                dest.Value.Value.ShouldBe(5);
            }
        }
        
        
        public class When_scanning_by_assembly_with_complex_generic_nested_closed_generic_destination : NonValidatingSpecBase
        {
            public class Source {
                public int Value { get; set; }
            }
            
            public class Demo<T> {
                public T Value { get; set; }
            }
            
            public class Dest<T, TOther> 
                where T : notnull
                where TOther : class
            {
                public T Value { get; set; }
                public TOther Other { get; set; }
        
                private class GenericMapping : Profile
                {
                    public GenericMapping() {
                        CreateMap<Source, Dest<int, Demo<int>>>()
                            .ForMember(dest => dest.Other, opt => opt.MapFrom(src => new Demo<int> { Value = src.Value }));
                    }
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg => {
                cfg.AddMaps(typeof(When_scanning_by_assembly_with_nested_closed_generic_destination).Assembly);
            });

            [Fact]
            public void Should_map() {
                var source = new Source { Value = 5 };
                var dest = Mapper.Map<Dest<int, Demo<int>>>(source);

                dest.Value.ShouldBe(5);
                dest.Other.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully() {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid<Source, Dest<int, Demo<int>>>);
            }
        }
    }
}