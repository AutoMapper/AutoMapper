using System;
using Shouldly;
using System.Linq;
using System.Collections.Generic;
using AutoMapper.UnitTests;
using System.Data.Entity;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq.Expressions;
    using QueryableExtensions;

    public class MapObjectPropertyFromSubQueryExplicitExpansion : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductModel>()
                .ForMember(d => d.Price, o =>
                {
                    o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault());
                    o.ExplicitExpansion();
                });
            cfg.CreateMap<Article, PriceModel>()
                .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
        });

        [Fact]
        public void Should_map_ok()
        {
            using(var context = new ClientContext())
            {
                var projection = context.Products.ProjectTo<ProductModel>(Configuration);
                var counter = new FirstOrDefaultCounter();
                counter.Visit(projection.Expression);
                counter.Count.ShouldBe(0);
                var productModel = projection.First();
                productModel.Price.ShouldBeNull();
                productModel.Id.ShouldBe(1);
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
            public int Value { get; }
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
                counter.Count.ShouldBe(1);
                var productModel = projection.First();
                productModel.Price.RegionId.ShouldBe((short)1);
                productModel.Price.IsDefault.ShouldBeTrue();
                productModel.Price.Id.ShouldBe(1);
                productModel.Id.ShouldBe(1);
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
            public int Value { get; }
            [NotMapped]
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
                counter.Count.ShouldBe(2);
                var productArticleModel = projection.First();
                var productModel = productArticleModel.Product;
                productModel.Price.RegionId.ShouldBe((short)1);
                productModel.Price.IsDefault.ShouldBeTrue();
                productModel.Price.Id.ShouldBe(1);
                productModel.Id.ShouldBe(1);
                var otherProductModel = productArticleModel.OtherProduct;
                otherProductModel.Price.RegionId.ShouldBe((short)1);
                otherProductModel.Price.IsDefault.ShouldBeTrue();
                otherProductModel.Price.Id.ShouldBe(2);
                otherProductModel.Id.ShouldBe(2);
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
            public Product OtherProduct { get; set; }
        }

        public class ProductArticleModel
        {
            public int Id { get; set; }
            public ProductModel Product { get; set; }
            public ProductModel OtherProduct { get; set; }
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
                var product1 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
                var product2 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 2 } } });
                context.ProductArticles.Add(new ProductArticle { Product = product1, OtherProduct = product2 });
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

    public class MapObjectPropertyFromSubQueryWithCollection : AutoMapperSpecBase
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
                counter.Count.ShouldBe(1);
                var productModel = projection.First().Products.First();
                productModel.Price.RegionId.ShouldBe((short)1);
                productModel.Price.IsDefault.ShouldBeTrue();
                productModel.Price.Id.ShouldBe(1);
                productModel.Id.ShouldBe(1);
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
            public ICollection<Product> Products { get; set; }
        }

        public class ProductArticleModel
        {
            public int Id { get; set; }
            public ICollection<ProductModel> Products { get; set; }
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
                context.ProductArticles.Add(new ProductArticle { Products = new[] { product } });
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

    public class MapObjectPropertyFromSubQueryWithCollectionSameName : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ProductArticle, ProductArticleModel>();
            cfg.CreateMap<Product, ProductModel>()
                .ForMember(d=>d.ArticlesModel, o=>o.MapFrom(s=>s))
                .ForMember(d => d.Articles, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
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
                counter.Count.ShouldBe(1);
                var productModel = projection.First().Products.First();
                Check(productModel.Articles);
                productModel.Id.ShouldBe(1);
                productModel.ArticlesCount.ShouldBe(1);
                productModel.ArticlesModel.Articles.Count.ShouldBe(1);
                Check(productModel.ArticlesModel.Articles.Single());
            }
        }

        private static void Check(PriceModel priceModel)
        {
            priceModel.RegionId.ShouldBe((short)1);
            priceModel.IsDefault.ShouldBeTrue();
            priceModel.Id.ShouldBe(1);
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
            public ICollection<Product> Products { get; set; }
        }

        public class ProductArticleModel
        {
            public int Id { get; set; }
            public ICollection<ProductModel> Products { get; set; }
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
            public PriceModel Articles { get; set; }
            public int ArticlesCount { get; set; }
            public ArticlesModel ArticlesModel { get; set; }
        }

        public class ArticlesModel
        {
            public ICollection<PriceModel> Articles { get; set; }
        }

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
                context.ProductArticles.Add(new ProductArticle { Products = new[] { product } });
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

    public class SubQueryWithMapFromNullable : AutoMapperSpecBase
    {
        // Source Types
        public class Cable
        {
            public int CableId { get; set; }
            public ICollection<CableEnd> Ends { get; set; } = new List<CableEnd>();
        }

        public class CableEnd
        {
            [ForeignKey(nameof(CrossConnectId))]
            public virtual Cable CrossConnect { get; set; }
            [Column(Order = 0), Key]
            public int CrossConnectId { get; set; }
            [Column(Order = 1), Key]
            public string Name { get; set; }
            [ForeignKey(nameof(RackId))]
            public virtual Rack Rack { get; set; }
            public int? RackId { get; set; }
        }

        public class DataHall
        {
            public int DataHallId { get; set; }
            public int DataCentreId { get; set; }
            public ICollection<Rack> Racks { get; set; } = new List<Rack>();
        }

        public class Rack
        {
            public int RackId { get; set; }
            [ForeignKey(nameof(DataHallId))]
            public virtual DataHall DataHall { get; set; }
            public int DataHallId { get; set; }
        }

        // Dest Types
        public class CableListModel
        {
            public int CableId { get; set; }
            public CableEndModel AEnd { get; set; }
            public CableEndModel AnotherEnd { get; set; }
        }

        public class CableEndModel
        {
            public string Name { get; set; }
            public int? DataHallId { get; set; }
        }

        class ClientContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                Database.SetInitializer(new Initializer());
            }

            public DbSet<Cable> Cables { get; set; }
            public DbSet<CableEnd> CableEnds { get; set; }
            public DbSet<DataHall> DataHalls { get; set; }
        }

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                var rack = new Rack();
                var dh = new DataHall { DataCentreId = 10, Racks = { rack } };
                context.DataHalls.Add(dh);
                var cable = new Cable
                {
                    Ends = new List<CableEnd>()
                    {
                        new CableEnd { Name = "A", Rack = rack},
                        new CableEnd { Name = "B" },
                    }
                };
                context.Cables.Add(cable);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CableEnd, CableEndModel>().ForMember(dest => dest.DataHallId, opt => opt.MapFrom(src => src.Rack.DataHall.DataCentreId));
            cfg.CreateMap<Cable, CableListModel>()
                .ForMember(dest => dest.AEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "A")))
                .ForMember(dest => dest.AnotherEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "B")));
        });

        [Fact]
        public void Should_project_ok()
        {
            using(var context = new ClientContext())
            {
                var projection = context.Cables.ProjectTo<CableListModel>(Configuration);
                var result = projection.Single();
                result.AEnd.DataHallId.ShouldBe(10);
                result.AnotherEnd.DataHallId.ShouldBeNull();
            }
        }
    }
}