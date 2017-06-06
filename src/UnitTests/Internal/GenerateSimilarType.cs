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

        [Fact]
        public void Should_work()
        {
            var sourceType = typeof(Product);
            var similarType = ProxyGenerator.GetSimilarType(sourceType);
            var sourceProperties = GetProperties(similarType);
            var similarTypeProperties = GetProperties(similarType);
            similarTypeProperties.SequenceEqual(sourceProperties).ShouldBeTrue();

            dynamic instance = Activator.CreateInstance(similarType);
            instance.Id = 12;
            instance.Name = "John";
            instance.ECommercePublished = true;
            var articles = new Article[3];
            instance.Articles = articles;

            Assert.Equal(12, instance.Id);
            Assert.Equal("John", instance.Name);
            Assert.Equal(true, instance.ECommercePublished);
            Assert.Equal(articles, instance.Articles);
        }

        public IEnumerable<object> GetProperties(Type type)
        {
            return type.GetProperties().OrderBy(p => p.Name).Select(p => new { p.Name, p.PropertyType });
        }
    }
}