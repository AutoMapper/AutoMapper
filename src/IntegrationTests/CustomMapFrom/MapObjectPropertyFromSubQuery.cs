using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.CustomMapFrom;

public class MemberWithSubQueryProjections : AutoMapperSpecBase, IAsyncLifetime
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class CustomerViewModel
    {
        public CustomerNameModel Name { get; set; }
        public ItemModel FirstItem { get; set; }
    }
    public class CustomerNameModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src : null))
            .ForMember(dst => dst.FirstItem, opt => opt.MapFrom(src => src.Items.FirstOrDefault()));
        cfg.CreateProjection<Customer, CustomerNameModel>();
        cfg.CreateProjection<Item, ItemModel>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = new Context())
        {
            var resultQuery = ProjectTo<CustomerViewModel>(context.Customers);
            var result = resultQuery.Single();
            result.Name.FirstName.ShouldBe("Bob");
            result.Name.LastName.ShouldBe("Smith");
            result.FirstItem.Id.ShouldBe(1);
            result.FirstItem.Code.ShouldBe(1);
        }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
public class MemberWithSubQueryProjectionsNoMap : AutoMapperSpecBase, IAsyncLifetime
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class CustomerViewModel
    {
        public string Name { get; set; }
        public ItemModel FirstItem { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src.LastName : null))
            .ForMember(dst => dst.FirstItem, opt => opt.MapFrom(src => src.Items.FirstOrDefault()));
        cfg.CreateProjection<Item, ItemModel>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = new Context())
        {
            var resultQuery = ProjectTo<CustomerViewModel>(context.Customers);
            var result = resultQuery.Single();
            result.Name.ShouldBe("Smith");
            result.FirstItem.Id.ShouldBe(1);
            result.FirstItem.Code.ShouldBe(1);
        }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
public class MapObjectPropertyFromSubQueryTypeNameMax : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products);
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
            if (node.Method.Name == "FirstOrDefault")
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
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName1 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName2 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName3 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName4 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName5 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName6 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName7 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName8 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName9 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName10 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName11 { get; set; }
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
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName1 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName2 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName3 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName4 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName5 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName6 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName7 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName8 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName9 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName10 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName11 { get; set; }
    }

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MapObjectPropertyFromSubQueryExplicitExpansion : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o =>
            {
                o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault());
                o.ExplicitExpansion();
            });
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_map_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products);
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
            if (node.Method.Name == "FirstOrDefault")
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

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Article> Articles { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MapObjectPropertyFromSubQuery : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products);
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
            if (node.Method.Name == "FirstOrDefault")
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

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MapObjectPropertyFromSubQueryWithInnerObject : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<ProductArticle, ProductArticleModel>();
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles);
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

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product1 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            var product2 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 2 } } });
            context.ProductArticles.Add(new ProductArticle { Product = product1.Entity, OtherProduct = product2.Entity });
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MapObjectPropertyFromSubQueryWithCollection : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<ProductArticle, ProductArticleModel>();
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles);
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
            if (node.Method.Name == "FirstOrDefault")
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

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticle { Products = new[] { product.Entity } });
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MapObjectPropertyFromSubQueryWithCollectionSameName : NonValidatingSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<ProductArticle, ProductArticleModel>();
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.ArticlesModel, o => o.MapFrom(s => s))
            .ForMember(d => d.Articles, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Product, ArticlesModel>();
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles);
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
            if (node.Method.Name == "FirstOrDefault")
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

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticle { Products = new[] { product.Entity } });
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }
    }
    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class SubQueryWithMapFromNullable : AutoMapperSpecBase, IAsyncLifetime
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

    class ClientContext : LocalDbContext
    {
        public DbSet<Cable> Cables { get; set; }
        public DbSet<CableEnd> CableEnds { get; set; }
        public DbSet<DataHall> DataHalls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CableEnd>().HasKey(c => new { c.CrossConnectId, c.Name });
        }
    }

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<CableEnd, CableEndModel>().ForMember(dest => dest.DataHallId, opt => opt.MapFrom(src => src.Rack.DataHall.DataCentreId));
        cfg.CreateProjection<Cable, CableListModel>()
            .ForMember(dest => dest.AEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "A")))
            .ForMember(dest => dest.AnotherEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "B")));
    });

    [Fact]
    public void Should_project_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<CableListModel>(context.Cables);
            var result = projection.Single();
            result.AEnd.DataHallId.ShouldBe(10);
            result.AnotherEnd.DataHallId.ShouldBeNull();
        }
    }
    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MapObjectPropertyFromSubQueryCustomSource : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Owner, OwnerDto>();
        cfg.CreateProjection<Brand, BrandDto>()
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owners.FirstOrDefault()));
        cfg.CreateProjection<ProductReview, ProductReviewDto>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Product.Brand));
    });

    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Brand
    {
        public int Id { get; set; }
        public List<Owner> Owners { get; set; } = new List<Owner>();
    }
    public class Product
    {
        public int Id { get; set; }
        public Brand Brand { get; set; }
    }
    public class ProductReview
    {
        public int Id { get; set; }
        public Product Product { get; set; }
    }
    /* Destination types */
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public BrandDto Brand { get; set; }
    }
    public class BrandDto
    {
        public int Id { get; set; }
        public OwnerDto Owner { get; set; }
    }
    public class OwnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
    }

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.ProductReviews.Add(new ProductReview
            { Product = new Product { Brand = new Brand { Owners = { new Owner { Name = "Owner" } } } } });
            context.ProductReviews.Add(new ProductReview
            { Product = new Product { Brand = new Brand { Owners = { new Owner() } } } });
            context.ProductReviews.Add(new ProductReview { Product = new Product() });
        }
    }

    [Fact]
    public void Should_project_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductReviewDto>(context.ProductReviews);
            var results = projection.ToArray();
            results.Any(result => result?.Brand?.Owner?.Name == "Owner").ShouldBeTrue();
            results.Any(result => result?.Brand?.Owner == null).ShouldBeTrue();
            results.Any(result => result?.Brand == null).ShouldBeTrue();
        }
    }
    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class MemberWithSubQueryIdentity : AutoMapperSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new MapperConfiguration(cfg =>
    {
        cfg.CreateProjection<AEntity, Dto>()
            .ForMember(dst => dst.DtoSubWrapper, opt => opt.MapFrom(src => src));
        cfg.CreateProjection<AEntity, DtoSubWrapper>()
            .ForMember(dst => dst.DtoSub, opt => opt.MapFrom(src => src.BEntity.CEntities.FirstOrDefault(x => x.Id == src.CEntityId)));
        cfg.CreateProjection<CEntity, DtoSub>();
    });
    [Fact]
    public void Should_work()
    {
        var query = ProjectTo<Dto>(new ClientContext().AEntities);
        var result = query.Single();
        result.DtoSubWrapper.DtoSub.ShouldNotBeNull();
        result.DtoSubWrapper.DtoSub.SubString.ShouldBe("Test");
    }
    public class Dto
    {
        public int Id { get; set; }
        public DtoSubWrapper DtoSubWrapper { get; set; }
    }
    public class DtoSubWrapper
    {
        public DtoSub DtoSub { get; set; }
    }
    public class DtoSub
    {
        public int Id { get; set; }
        public string SubString { get; set; }
    }
    public class AEntity
    {
        public int Id { get; set; }
        public int BEntityId { get; set; }
        public int CEntityId { get; set; }
        public BEntity BEntity { get; set; }
    }
    public class BEntity
    {
        public int Id { get; set; }
        public ICollection<CEntity> CEntities { get; set; }
    }
    public class CEntity
    {
        public int Id { get; set; }
        public int BEntityId { get; set; }
        public string SubString { get; set; }
        public BEntity BEntity { get; set; }
    }
    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.AEntities.Add(new AEntity
            {
                CEntityId = 6,
                BEntity = new BEntity
                {
                    CEntities = new List<CEntity>
                        {
                            new CEntity
                            {
                                Id = 6,
                                BEntityId = 1,
                                SubString = "Test"
                            }
                        }
                },
            });
        }
    }
    class ClientContext : LocalDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AEntity>()
                .HasOne(x => x.BEntity)
                .WithMany()
                .HasForeignKey(x => x.BEntityId);

            modelBuilder.Entity<BEntity>()
                .HasMany(x => x.CEntities)
                .WithOne(x => x.BEntity)
                .HasForeignKey(x => x.BEntityId);

            modelBuilder.Entity<CEntity>()
                .Property(x => x.Id)
                .ValueGeneratedNever();
        }
        public DbSet<AEntity> AEntities { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}