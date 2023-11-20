using AutoMapper.Execution;

namespace AutoMapper.UnitTests;

public class GenerateSimilarType
{
    public partial record struct Article
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public bool IsDefault { get; set; }
        public short NationId { get; set; }
        public Product Product { get; set; }
    }

    public partial class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool ECommercePublished { get; set; }
        public virtual ICollection<Article> Articles { get; set; }
    }

    class ExtraProduct : Product
    {
        public long Long { get; set; }
        public short Short { get; set; }
        public Article Article { get; set; }
    }

    [Fact]
    public void Should_work()
    {
        var extraProperties = typeof(ExtraProduct).GetProperties().Except(typeof(Product).GetProperties()).Select(p => new PropertyDescription(p));
        var similarType = ProxyGenerator.GetSimilarType(typeof(Product), extraProperties);

        similarType.Assembly.IsDynamic.ShouldBeTrue();
        var sourceProperties = GetProperties(typeof(ExtraProduct));
        var similarTypeProperties = GetProperties(similarType);
        similarTypeProperties.SequenceEqual(sourceProperties).ShouldBeTrue();

        dynamic instance = Activator.CreateInstance(similarType);
        instance.Id = 12;
        instance.Name = "John";
        instance.ECommercePublished = true;
        instance.Short = short.MaxValue;
        instance.Long = long.MaxValue;
        var articles = new Article[] { new Article(), default, default };
        instance.Articles = articles;
        instance.Article = articles[0];

        Assert.Equal(12, instance.Id);
        Assert.Equal("John", instance.Name);
        Assert.Equal(true, instance.ECommercePublished);
        Assert.Equal(short.MaxValue, instance.Short);
        Assert.Equal(long.MaxValue, instance.Long);
        Assert.Equal(articles, instance.Articles);
        Assert.Equal(articles[0], instance.Article);
    }

    public IEnumerable<(string Name, Type PropertyType)> GetProperties(Type type) =>
        type.GetProperties().OrderBy(p => p.Name).Select(p => (p.Name, p.PropertyType));
}
