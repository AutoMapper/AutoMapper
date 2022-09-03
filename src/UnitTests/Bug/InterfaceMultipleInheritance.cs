namespace AutoMapper.UnitTests.Bug
{
    namespace InterfaceMultipleInheritance
    {
        public class InterfaceMultipleInheritanceBug1036 : AutoMapperSpecBase
        {
            private MapTo _destination;

            public interface IMapFrom
            {
                IMapFromElement Element { get; }
            }

            public interface IMapFromElement
            {
                string Prop { get; }
            }

            public interface IMapFromElementDerived1 : IMapFromElement
            {
                string Prop2 { get; }
            }

            public interface IMapFromElementDerived2 : IMapFromElement
            {
            }

            public interface IMapFromElementDerivedBoth : IMapFromElementDerived1, IMapFromElementDerived2
            {
            }

            public interface IMapToElementWritable 
            {
                string Prop { get; set; }
            }

            public interface IMapToElementWritableDerived : IMapToElementWritable
            {
                string Prop2 { get; set; }
            }

            public class MapFrom : IMapFrom
            {
                public MapFromElement Element { get; set; }
                IMapFromElement IMapFrom.Element => Element;
            }

            public class MapFromElement : IMapFromElement
            {
                public string Prop { get; set; }
            }

            public class MapFromElementDerived : MapFromElement, IMapFromElementDerivedBoth
            {
                public new string Prop { get; set; }
                public string Prop2 { get; set; }
            }

            public class MapTo
            {
                public IMapToElementWritable Element { get; set; }
            }

            public abstract class MapToElement : IMapToElementWritable
            {
                public string Prop { get; set; }
            }

            public class MapToElementDerived : MapToElement, IMapToElementWritableDerived
            {
                public string Prop2 { get; set; }
            }


            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<IMapFrom, MapTo>();
                cfg.CreateMap<IMapFromElement, IMapToElementWritable>()
                .Include<IMapFromElementDerived1, IMapToElementWritableDerived>();

                cfg.CreateMap<IMapFromElementDerived1, IMapToElementWritableDerived>()
                    .ConstructUsing(src => new MapToElementDerived());
            });

            protected override void Because_of()
            {
                var source = new MapFrom
                {
                    Element = new MapFromElementDerived { Prop = "PROP1", Prop2 = "PROP2" }
                };

                _destination = Mapper.Map<MapTo>(source);
            }

            [Fact]
            public void Should_Map_UsingDerivedInterface()
            {
                var element = (IMapToElementWritableDerived)_destination.Element;
                element.Prop2.ShouldBe("PROP2");
            }
        }

        public class InterfaceMultipleInheritanceBug1016 : AutoMapperSpecBase
        {
            private class4DTO _destination;

            public abstract class class1 : iclass1
            {
                public string prop1 { get; set; }
            }

            public class class2 : class1, iclass2
            {
                public string prop2 { get; set; }
            }

            public class class3 : class2, iclass3
            {
                public string prop3 { get; set; }
            }

            public class class4 : class3, iclass4
            {
                public string prop4 { get; set; }
            }

            public abstract class class1DTO : iclass1DTO
            {
                public string prop1 { get; set; }
            }

            public class class2DTO : class1DTO, iclass2DTO
            {
                public string prop2 { get; set; }
            }

            public class class3DTO : class2DTO, iclass3DTO
            {
                public string prop3 { get; set; }
            }

            public class class4DTO : class3DTO, iclass4DTO
            {
                public string prop4 { get; set; }
            }

            public interface iclass1
            {
                string prop1 { get; set; }
            }

            public interface iclass2 : iclass1
            {
                string prop2 { get; set; }
            }

            public interface iclass3 : iclass2
            {
                string prop3 { get; set; }
            }

            public interface iclass4 : iclass3
            {
                string prop4 { get; set; }
            }

            public interface iclass1DTO
            {
                string prop1 { get; set; }
            }

            public interface iclass2DTO : iclass1DTO
            {
                string prop2 { get; set; }
            }

            public interface iclass3DTO : iclass2DTO
            {
                string prop3 { get; set; }
            }

            public interface iclass4DTO : iclass3DTO
            {
                string prop4 { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<iclass1, iclass1DTO>()
                    .Include<iclass2, iclass2DTO>()
                    .Include<iclass3, iclass3DTO>()
                    .Include<iclass4, iclass4DTO>();
                cfg.CreateMap<iclass2, iclass2DTO>();
                cfg.CreateMap<iclass3, iclass3DTO>();
                cfg.CreateMap<iclass4, iclass4DTO>()
                    .ConstructUsing(src => new class4DTO());
            });

            protected override void Because_of()
            {
                iclass4 source = new class4();
                source.prop1 = "PROP1";
                source.prop2 = "PROP2";
                source.prop3 = "PROP3";
                source.prop4 = "PROP4";
                _destination = new class4DTO();

                Mapper.Map<iclass4, iclass4DTO>(source, _destination);
            }

            [Fact]
            public void Should_Map_UsingDerivedInterface()
            {
                _destination.prop4.ShouldBe("PROP4");
            }
        }
    }
}