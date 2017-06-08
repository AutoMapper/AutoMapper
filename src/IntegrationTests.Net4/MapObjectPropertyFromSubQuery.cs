using System;
using Should;
using System.Linq;
using System.Collections.Generic;
using AutoMapper.UnitTests;
using System.Data.Entity;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using System.Linq.Expressions;
    using QueryableExtensions;

    public class MapObjectPropertyFromSubQuery : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<Product, ProductModel>()
                .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
            cfg.CreateMap<Article, PriceModel>()
                .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
        });

        [Fact]
        public void Should_cache_the_subquery()
        {
            using(var context = new ClientContext())
            {
                var projection = context.Products.ProjectTo<ProductModel>(Configuration);
                var counter = new FirstOrDefaultCounter();
                counter.Visit(projection.Expression);
                counter.Count.ShouldEqual(1);
                var productModel = projection.First();
                productModel.Price.RegionId.ShouldEqual((short)1);
                productModel.Price.IsDefault.ShouldBeTrue();
                productModel.Price.Id.ShouldEqual(1);
                productModel.Id.ShouldEqual(1);
            }
        }

        class FirstOrDefaultCounter : ExpressionVisitor
        {
            public int Count;

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if(node.Method.Name == "FirstOrDefault")
                {
                    Count++;
                }
                return base.VisitMethodCall(node);
            }
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

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            }
        }

        class ClientContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                Database.SetInitializer(new Initializer());
            }

            public DbSet<Product> Products { get; set; }
        }
    }

    public class MapObjectPropertyFromSubQueryWithInnerObject : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ProductArticle, ProductArticleModel>();
            cfg.CreateMap<Product, ProductModel>()
                .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
            cfg.CreateMap<Article, PriceModel>()
                .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
        });

        [Fact]
        public void Should_cache_the_subquery()
        {
            using(var context = new ClientContext())
            {
                var projection = context.ProductArticles.ProjectTo<ProductArticleModel>(Configuration);
                var counter = new FirstOrDefaultCounter();
                counter.Visit(projection.Expression);
                counter.Count.ShouldEqual(1);
                var productModel = projection.First().Product;
                productModel.Price.RegionId.ShouldEqual((short)1);
                productModel.Price.IsDefault.ShouldBeTrue();
                productModel.Price.Id.ShouldEqual(1);
                productModel.Id.ShouldEqual(1);
            }
        }

        class FirstOrDefaultCounter : ExpressionVisitor
        {
            public int Count;

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if(node.Method.Name == "FirstOrDefault")
                {
                    Count++;
                }
                return base.VisitMethodCall(node);
            }
        }

        public class ProductArticle
        {
            public int Id { get; set; }
            public Product Product { get; set; }
        }

        public class ProductArticleModel
        {
            public int Id { get; set; }
            public ProductModel Product { get; set; }
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

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
                context.ProductArticles.Add(new ProductArticle { Product = product });
            }
        }

        class ClientContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                Database.SetInitializer(new Initializer());
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<ProductArticle> ProductArticles { get; set; }
        }
    }
}