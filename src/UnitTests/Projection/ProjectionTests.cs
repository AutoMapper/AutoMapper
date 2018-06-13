namespace AutoMapper.UnitTests.Projection
{
    using System.Linq;
    using Xunit;
    using Shouldly;
    using AutoMapper;
    using QueryableExtensions;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class InMemoryMapObjectPropertyFromSubQuery : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductModel>()
                .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
            cfg.CreateMap<Article, PriceModel>()
                .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
        });

        [Fact]
        public void Should_cache_the_subquery()
        {
            var products = new[] { new Product { Id = 1, ECommercePublished = true, Articles = new[] { new Article { Id = 1, IsDefault = true, NationId = 1, ProductId = 1 } } } }.AsQueryable();
            var projection = products.ProjectTo<ProductModel>(Configuration);
            var productModel = projection.First();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Price.Id.ShouldBe(1);
            productModel.Id.ShouldBe(1);
        }

        public partial class Article
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public bool IsDefault { get; set; }
            public short NationId { get; set; }
            public virtual Product Product { get; set; }
        }

        public partial class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool ECommercePublished { get; set; }
            public virtual ICollection<Article> Articles { get; set; }
            public int Value { get; }
            public int NotMappedValue { get; set; }
            public virtual List<Article> OtherArticles { get; }
        }

        public class PriceModel
        {
            public int Id { get; set; }
            public short RegionId { get; set; }
            public bool IsDefault { get; set; }
        }

        public class ProductModel
        {
            public int Id { get; set; }
            public PriceModel Price { get; set; }
        }
    }


    public class ProjectionTests
    {
        string _niceGreeting = "Hello";
        string _badGreeting = "GRRRRR";
        

        [Fact]
        public void Direct_assignability_shouldnt_trump_custom_projection() {
            var config = new MapperConfiguration(x => {
                x.CreateMap<string, string>()
                    .ProjectUsing(s => _niceGreeting);

                x.CreateMap<Source, Target>();
                x.CreateMap<SourceChild, TargetChild>();
            });

            var target = new[] { new Source() { Greeting = _badGreeting } }
                            .AsQueryable()
                            .ProjectTo<Target>(config)
                            .First();

            target.Greeting.ShouldBe(_niceGreeting);
        }


        [Fact]
        public void Root_is_subject_to_custom_projection() {
            var config = new MapperConfiguration(x => {
                x.CreateMap<Source, Target>()
                    .ProjectUsing(s => new Target() { Greeting = _niceGreeting });
            });

            var target = new[] { new Source() }
                            .AsQueryable()
                            .ProjectTo<Target>(config)
                            .First();

            target.Greeting.ShouldBe(_niceGreeting);
        }


        [Fact]
        public void Child_nodes_are_subject_to_custom_projection() {
            var config = new MapperConfiguration(x => {
                x.CreateMap<SourceChild, TargetChild>()
                    .ProjectUsing(s => new TargetChild() { Greeting = _niceGreeting });

                x.CreateMap<Source, Target>();
            });

            var target = new[] { new Source() }
                            .AsQueryable()
                            .ProjectTo<Target>(config)
                            .First();

            target.Child.Greeting.ShouldBe(_niceGreeting);
        }




        class Source
        {
            public string Greeting { get; set; }
            public int Number { get; set; }
            public SourceChild Child { get; set; }

            public Source() {
                Child = new SourceChild();
            }
        }

        class SourceChild
        {
            public string Greeting { get; set; }
        }


        class Target
        {
            public string Greeting { get; set; }
            public int? Number { get; set; }
            public TargetChild Child { get; set; }
        }

        class TargetChild
        {
            public string Greeting { get; set; }
        }
    }
}
