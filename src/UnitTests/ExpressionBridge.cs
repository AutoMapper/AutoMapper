using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Should;
using NUnit.Framework;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests
{
    namespace ExpressionBridge
    {
        public class SimpleProductDto
        {
            public string Name { set; get; }
            public string ProductSubcategoryName { set; get; }
            public string CategoryName { set; get; }
        }
        public class ExtendedProductDto
        {
            public string Name { set; get; }
            public string ProductSubcategoryName { set; get; }
            public string CategoryName { set; get; }
            public List<BillOfMaterialsDto> BOM { set; get; }
        }
        public class ComplexProductDto
        {
            public string Name { get; set; }
            public ProductSubcategoryDto ProductSubcategory { get; set; }
        }
        public class ProductSubcategoryDto
        {
            public string Name { get; set; }
            public ProductCategoryDto ProductCategory { get; set; }
        }
        public class ProductCategoryDto
        {
            public string Name { get; set; }
        }
        public class BillOfMaterialsDto
        {
            public int BillOfMaterialsID { set; get; }
        }

        public class Product
        {
            public string Name { get; set; }
            public ProductSubcategory ProductSubcategory { get; set; }
            public List<BillOfMaterials> BillOfMaterials { set; get; }
        }

        public class ProductSubcategory
        {
            public string Name { get; set; }
            public ProductCategory ProductCategory { get; set; }
        }

        public class ProductCategory
        {
            public string Name { get; set; }
        }

        public class BillOfMaterials
        {
            public int BillOfMaterialsID { set; get; }
        }

        [TestFixture]
        public class When_mapping_using_expressions : NonValidatingSpecBase
        {
            private List<Product> _products;
            private Expression<Func<Product, SimpleProductDto>> _simpleProductConversionLinq;
            private Expression<Func<Product, ExtendedProductDto>> _extendedProductConversionLinq;
            private List<SimpleProductDto> _simpleProducts;
            private List<ExtendedProductDto> _extendedProducts;

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Product, SimpleProductDto>()
                        .ForMember(m => m.CategoryName, dst => dst.MapFrom(p => p.ProductSubcategory.ProductCategory.Name));
                    cfg.CreateMap<Product, ExtendedProductDto>()
                        .ForMember(m => m.CategoryName, dst => dst.MapFrom(p => p.ProductSubcategory.ProductCategory.Name))
                        .ForMember(m => m.BOM, dst => dst.MapFrom(p => p.BillOfMaterials));
                    cfg.CreateMap<BillOfMaterials, BillOfMaterialsDto>();
                    cfg.CreateMap<Product, ComplexProductDto>();
                    cfg.CreateMap<ProductSubcategory, ProductSubcategoryDto>();
                    cfg.CreateMap<ProductCategory, ProductCategoryDto>();
                });

                _simpleProductConversionLinq = Mapper.CreateMapExpression<Product, SimpleProductDto>();
                _extendedProductConversionLinq = Mapper.CreateMapExpression<Product, ExtendedProductDto>();

                _products = new List<Product>()
                {
                    new Product
                    {
                        Name = "Foo",
                        ProductSubcategory = new ProductSubcategory
                        {
                            Name = "Bar",
                            ProductCategory = new ProductCategory
                            {
                                Name = "Baz"
                            }
                        },
                        BillOfMaterials = new List<BillOfMaterials>
                        {
                            new BillOfMaterials
                            {
                                BillOfMaterialsID = 5
                            }
                        }
                    }
                };
            }

            protected override void Because_of()
            {
                var queryable = _products.AsQueryable();

                _simpleProducts = queryable.Select(_simpleProductConversionLinq).ToList();

                _extendedProducts = queryable.Select(_extendedProductConversionLinq).ToList();

            }

            [Test]
            public void Should_map_and_flatten()
            {


                _simpleProducts.Count.ShouldEqual(1);
                _simpleProducts[0].Name.ShouldEqual("Foo");
                _simpleProducts[0].ProductSubcategoryName.ShouldEqual("Bar");
                _simpleProducts[0].CategoryName.ShouldEqual("Baz");

                _extendedProducts.Count.ShouldEqual(1);
                _extendedProducts[0].Name.ShouldEqual("Foo");
                _extendedProducts[0].ProductSubcategoryName.ShouldEqual("Bar");
                _extendedProducts[0].CategoryName.ShouldEqual("Baz");
                _extendedProducts[0].BOM.Count.ShouldEqual(1);
                _extendedProducts[0].BOM[0].BillOfMaterialsID.ShouldEqual(5);
            }
           
            [Test]
            public void Should_use_extension_methods()
            {

                
                var queryable = _products.AsQueryable();

                var simpleProducts = queryable.Project().To<SimpleProductDto>().ToList();

                simpleProducts.Count.ShouldEqual(1);
                simpleProducts[0].Name.ShouldEqual("Foo");
                simpleProducts[0].ProductSubcategoryName.ShouldEqual("Bar");
                simpleProducts[0].CategoryName.ShouldEqual("Baz");

                var extendedProducts = queryable.Project().To<ExtendedProductDto>().ToList();

                extendedProducts.Count.ShouldEqual(1);
                extendedProducts[0].Name.ShouldEqual("Foo");
                extendedProducts[0].ProductSubcategoryName.ShouldEqual("Bar");
                extendedProducts[0].CategoryName.ShouldEqual("Baz");
                extendedProducts[0].BOM.Count.ShouldEqual(1);
                extendedProducts[0].BOM[0].BillOfMaterialsID.ShouldEqual(5);

                var complexProducts = queryable.Project().To<ComplexProductDto>().ToList();

                complexProducts.Count.ShouldEqual(1);
                complexProducts[0].Name.ShouldEqual("Foo");
                complexProducts[0].ProductSubcategory.Name.ShouldEqual("Bar");
                complexProducts[0].ProductSubcategory.ProductCategory.Name.ShouldEqual("Baz");
            }
        }
    }
}