using System.Linq;
using Xunit;
using Shouldly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Projection
{
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
}