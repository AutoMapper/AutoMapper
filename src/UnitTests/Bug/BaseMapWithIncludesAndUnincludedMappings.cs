using Should.Core.Assertions;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class BaseMapWithIncludesAndUnincludedMappings
    {
        public class ADTO
        {
            public int Prop { get; set; }
        }

        public class BDTO : ADTO
        {
            public int PropB { get; set; }
        }

        public class BDTO2 : ADTO
        {

        }

        public class A
        {
            public int Prop { get; set; }
        }

        public class B : A
        {
            public int PropB { get; set; }
        }

        [Fact]
        public void base_has_include_of_source_but_mapping_with_both_sides_being_unmapped_types_from_the_base_fails()
        {
            Mapper.CreateMap<A, ADTO>().Include<B, BDTO>();
            Mapper.CreateMap<B, BDTO>();
            var a = Mapper.Map<A, ADTO>(new B(), new BDTO2()); // Throws invalid cast exception trying to convert BDTO2 to BDTO
        }
    }

    public class BaseMapChildProperty
    {
        public class Container
        {
            public BaseA Item { get; set; }
        }

        public class Container2
        {
            public BaseB Item { get; set; }
        }
        public class BaseA
        {
            public string Name { get; set; }
        }

        public class BaseB
        {
            public string Name { get; set; }
        }

        public class ProxyOfSubA : SubA
        {
        }
        public class SubA : BaseA
        {
            public string Description { get; set; }
        }

        public class SubB : BaseB
        {
            public string Description { get; set; }
        }

        [Fact]
        public void TestInitialiserProxyOfSub()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<SubA, SubB>();
                cfg.CreateMap<SubA, BaseB>().As<SubB>();
                cfg.CreateMap<Container, Container2>();
            });

            var mapped = Mapper.Map<Container, Container2>(new Container() { Item = new ProxyOfSubA() { Name = "Martin", Description = "Hello" } });
            Assert.IsType<SubB>(mapped.Item);

        }

        [Fact]
        public void TestInitialiserProxyOfSub1()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<BaseA, SubB>();
                cfg.CreateMap<BaseA, BaseB>();
            });

            var mapped = Mapper.Map<BaseA, SubB>(new ProxyOfSubA() { Name = "Martin", Description = "Hello" });
            Assert.IsType<SubB>(mapped);

        }

        [Fact]
        public void TestInitialiserProxyOfSubInclude()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<BaseA, BaseB>().Include<SubA, SubB>();
                cfg.CreateMap<SubA, SubB>();
                cfg.CreateMap<Container, Container2>();
            });

            var mapped = Mapper.Map<Container, Container2>(new Container() { Item = new ProxyOfSubA() { Name = "Martin", Description = "Hello" } });
            Assert.IsType<SubB>(mapped.Item);

        }
    }
}