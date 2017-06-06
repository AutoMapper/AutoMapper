using System.Linq;
using System.Collections.Generic;
using AutoMapper.Execution;
using Xunit;
using Should;
using System;
using Should.Core.Assertions;

namespace AutoMapper.UnitTests
{
    public class GenerateSimilarType
    {
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

        class ExtraProduct : Product
        {
            public long Long { get; set; }
            public short Short { get; set; }
        }

        [Fact]
        public void Should_work()
        {
            var extraProperties = typeof(ExtraProduct).GetProperties().Except(typeof(Product).GetProperties()).Select(p => new PropertyDescription(p)).ToArray();
            var similarType = ProxyGenerator.GetSimilarType(typeof(Product), extraProperties);

            var sourceProperties = GetProperties(typeof(ExtraProduct));
            var similarTypeProperties = GetProperties(similarType);
            similarTypeProperties.SequenceEqual(sourceProperties).ShouldBeTrue();

            dynamic instance = Activator.CreateInstance(similarType);
            instance.Id = 12;
            instance.Name = "John";
            instance.ECommercePublished = true;
            instance.Short = short.MaxValue;
            instance.Long = long.MaxValue;
            var articles = new Article[3];
            instance.Articles = articles;

            Assert.Equal(12, instance.Id);
            Assert.Equal("John", instance.Name);
            Assert.Equal(true, instance.ECommercePublished);
            Assert.Equal(short.MaxValue, instance.Short);
            Assert.Equal(long.MaxValue, instance.Long);
            Assert.Equal(articles, instance.Articles);
        }

        public IEnumerable<object> GetProperties(Type type)
        {
            return type.GetProperties().OrderBy(p => p.Name).Select(p => new { p.Name, p.PropertyType });
        }
    }
}