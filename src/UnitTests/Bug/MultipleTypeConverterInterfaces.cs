namespace AutoMapper.UnitTests.Bug;

public class When_specifying_a_type_converter_implementing_multiple_type_converter_interfaces : AutoMapperSpecBase
    {
        private DestinationFoo _resultFoo;
        private DestinationBar _resultBar;

        public class SourceFoo
        {
            public int SourceFooValue { get; set; }
        }

        public class DestinationFoo
        {
            public int DestinationFooValue { get; set; }
        }
        public class SourceBar
        {
            public int SourceBarValue { get; set; }
        }

        public class DestinationBar
        {
            public int DestinationBarValue { get; set; }
        }

        public class DualConverter : ITypeConverter<SourceFoo, DestinationFoo>,
                                     ITypeConverter<SourceBar, DestinationBar>
        {
            public DestinationFoo Convert(SourceFoo source, DestinationFoo destination, ResolutionContext context)
            {
                return new DestinationFoo { DestinationFooValue = source.SourceFooValue + 100 };
            }

            DestinationBar ITypeConverter<SourceBar, DestinationBar>.Convert(SourceBar source, DestinationBar destination, ResolutionContext context)
            {
                return new DestinationBar { DestinationBarValue = source.SourceBarValue + 1000 };
            }
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap(typeof (SourceFoo), typeof (DestinationFoo)).ConvertUsing(typeof (DualConverter));
            cfg.CreateMap(typeof (SourceBar), typeof (DestinationBar)).ConvertUsing(typeof (DualConverter));
        });

        protected override void Because_of()
        {
            _resultFoo = Mapper.Map<SourceFoo, DestinationFoo>(new SourceFoo { SourceFooValue = 5 });
            _resultBar = Mapper.Map<SourceBar, DestinationBar>(new SourceBar { SourceBarValue = 6 });
        }

        [Fact]
        public void Should_use_implicit_converter()
        {
            _resultFoo.DestinationFooValue.ShouldBe(105);
        }

        [Fact]
        public void Should_use_explicit_converter()
        {
            _resultBar.DestinationBarValue.ShouldBe(1006);
        }
    }