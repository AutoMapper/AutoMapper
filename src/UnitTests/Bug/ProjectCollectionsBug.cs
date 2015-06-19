namespace AutoMapper.UnitTests.Bug
{
    using System.Collections.Generic;

    namespace ProjectCollectionsBug
    {
        using System;
        using System.Linq;
        using QueryableExtensions;
        using Xunit;

        /// <summary>
        /// 
        /// </summary>
        public class A
        {
            /// <summary>
            /// 
            /// </summary>
            public int AP1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string AP2 { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class B
        {
            /// <summary>
            /// 
            /// </summary>
            public B()
            {
                BP2 = new HashSet<A>();
            }

            /// <summary>
            /// 
            /// </summary>
            public int BP1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public ICollection<A> BP2 { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class AEntity
        {
            /// <summary>
            /// 
            /// </summary>
            public int AP1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string AP2 { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class BEntity
        {
            /// <summary>
            /// 
            /// </summary>
            public BEntity()
            {
                BP2 = new HashSet<AEntity>();
            }

            /// <summary>
            /// 
            /// </summary>
            public int BP1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public ICollection<AEntity> BP2 { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Bug
        {
            [Fact]
            public void Should_not_throw_exception()
            {
                Mapper.CreateMap<BEntity, B>();
                Mapper.CreateMap<AEntity, A>();
                Mapper.AssertConfigurationIsValid();

                var be = new BEntity {BP1 = 3};
                be.BP2.Add(new AEntity {AP1 = 1, AP2 = "hello"});
                be.BP2.Add(new AEntity {AP1 = 2, AP2 = "two"});

                var b = Mapper.Map<BEntity, B>(be);

                var belist = new List<BEntity> {be};
                var bei = belist.AsQueryable();
                typeof (Exception).ShouldNotBeThrownBy(() => bei.Project().To<B>());
            }
        }
    }
}