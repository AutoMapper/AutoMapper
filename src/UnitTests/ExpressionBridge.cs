using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.UnitTests
{
    public static class ExpressionBuilderExtensions
    {
        public static Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(this IProjectionBuilder expressionBuilder) =>
            (Expression<Func<TSource, TDestination>>)expressionBuilder.GetProjection(typeof(TSource), typeof(TDestination), null, Array.Empty<MemberPath>()).Projection;
    }
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
    public class AbstractProductDto
    {
        public string Name { set; get; }
        public string ProductSubcategoryName { set; get; }
        public string CategoryName { set; get; }
        public List<ProductTypeDto> Types { get; set; }
    }
    public abstract class ProductTypeDto { }
    public class ProdTypeA : ProductTypeDto {}
    public class ProdTypeB : ProductTypeDto {}

    public class ProductTypeConverter : ITypeConverter<ProductType, ProductTypeDto>
    {
        public ProductTypeDto Convert(ProductType source, ProductTypeDto destination, ResolutionContext context)
        {
            if (source.Name == "A")
                return new ProdTypeA();
            if (source.Name == "B")
                return new ProdTypeB();
            throw new ArgumentException();
        }
    }


    public class ProductType
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
        public List<ProductType> Types { get; set; }
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
    public class When_mapping_using_expressions : NonValidatingSpecBase
    {
        private List<Product> _products;
        private Expression<Func<Product, SimpleProductDto>> _simpleProductConversionLinq;
        private Expression<Func<Product, ExtendedProductDto>> _extendedProductConversionLinq;
        private Expression<Func<Product, AbstractProductDto>> _abstractProductConversionLinq;
        private List<SimpleProductDto> _simpleProducts;
        private List<ExtendedProductDto> _extendedProducts;
        private MapperConfiguration _config;

        protected override void Because_of()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateProjection<Product, SimpleProductDto>()
                    .ForMember(m => m.CategoryName, dst => dst.MapFrom(p => p.ProductSubcategory.ProductCategory.Name));
                cfg.CreateProjection<Product, ExtendedProductDto>()
                    .ForMember(m => m.CategoryName, dst => dst.MapFrom(p => p.ProductSubcategory.ProductCategory.Name))
                    .ForMember(m => m.BOM, dst => dst.MapFrom(p => p.BillOfMaterials));
                cfg.CreateProjection<BillOfMaterials, BillOfMaterialsDto>();
                cfg.CreateProjection<Product, ComplexProductDto>();
                cfg.CreateProjection<ProductSubcategory, ProductSubcategoryDto>();
                cfg.CreateProjection<ProductCategory, ProductCategoryDto>();
                cfg.CreateProjection<Product, AbstractProductDto>();
                cfg.CreateMap<ProductType, ProductTypeDto>()
                    //.ConvertUsing(x => ProductTypeDto.GetProdType(x));
                    .ConvertUsing<ProductTypeConverter>();
            });
            _simpleProductConversionLinq = _config.Internal().ProjectionBuilder.GetMapExpression<Product, SimpleProductDto>();
            _extendedProductConversionLinq = _config.Internal().ProjectionBuilder.GetMapExpression<Product, ExtendedProductDto>();
            _abstractProductConversionLinq = _config.Internal().ProjectionBuilder.GetMapExpression<Product, AbstractProductDto>();

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
                    ,
                    Types = new List<ProductType>
                                {
                                    new ProductType() { Name = "A" },
                                    new ProductType() { Name = "B" },
                                    new ProductType() { Name = "A" }
                                }
                }
            };
            var queryable = _products.AsQueryable();

            _simpleProducts = queryable.Select(_simpleProductConversionLinq).ToList();

            _extendedProducts = queryable.Select(_extendedProductConversionLinq).ToList();
        }

        [Fact]
        public void Should_map_and_flatten()
        {


            _simpleProducts.Count.ShouldBe(1);
            _simpleProducts[0].Name.ShouldBe("Foo");
            _simpleProducts[0].ProductSubcategoryName.ShouldBe("Bar");
            _simpleProducts[0].CategoryName.ShouldBe("Baz");

            _extendedProducts.Count.ShouldBe(1);
            _extendedProducts[0].Name.ShouldBe("Foo");
            _extendedProducts[0].ProductSubcategoryName.ShouldBe("Bar");
            _extendedProducts[0].CategoryName.ShouldBe("Baz");
            _extendedProducts[0].BOM.Count.ShouldBe(1);
            _extendedProducts[0].BOM[0].BillOfMaterialsID.ShouldBe(5);
        }
           
        [Fact]
        public void Should_use_extension_methods()
        {

                
            var queryable = _products.AsQueryable();

            var simpleProducts = queryable.ProjectTo<SimpleProductDto>(_config).ToList();

            simpleProducts.Count.ShouldBe(1);
            simpleProducts[0].Name.ShouldBe("Foo");
            simpleProducts[0].ProductSubcategoryName.ShouldBe("Bar");
            simpleProducts[0].CategoryName.ShouldBe("Baz");

            var extendedProducts = queryable.ProjectTo<ExtendedProductDto>(_config).ToList();

            extendedProducts.Count.ShouldBe(1);
            extendedProducts[0].Name.ShouldBe("Foo");
            extendedProducts[0].ProductSubcategoryName.ShouldBe("Bar");
            extendedProducts[0].CategoryName.ShouldBe("Baz");
            extendedProducts[0].BOM.Count.ShouldBe(1);
            extendedProducts[0].BOM[0].BillOfMaterialsID.ShouldBe(5);

            var complexProducts = queryable.ProjectTo<ComplexProductDto>(_config).ToList();

            complexProducts.Count.ShouldBe(1);
            complexProducts[0].Name.ShouldBe("Foo");
            complexProducts[0].ProductSubcategory.Name.ShouldBe("Bar");
            complexProducts[0].ProductSubcategory.ProductCategory.Name.ShouldBe("Baz");
        }
    }

    namespace CircularReferences
    {
        public class A
        {
            public int AP1 { get; set; }
            public string AP2 { get; set; }
            public virtual B B { get; set; }
        }

        public class B
        {
            public B()
            {
                BP2 = new HashSet<A>();
            }
            public int BP1 { get; set; }
            public virtual ICollection<A> BP2 { get; set; }
        }

        public class AEntity
        {
            public int AP1 { get; set; }
            public string AP2 { get; set; }
            public virtual BEntity B { get; set; }
        }
        public class BEntity
        {
            public BEntity()
            {
                BP2 = new HashSet<AEntity>();
            }
            public int BP1 { get; set; }
            public virtual ICollection<AEntity> BP2 { get; set; }
        }

        public class C
        {
            public C Value { get; set; }
        }

        public class When_mapping_circular_references : AutoMapperSpecBase
        {
            private IQueryable<BEntity> _bei;

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<BEntity, B>().MaxDepth(3);
                cfg.CreateProjection<AEntity, A>().MaxDepth(3);
            });

            protected override void Because_of()
            {
                var be = new BEntity();
                be.BP1 = 3;
                be.BP2.Add(new AEntity() { AP1 = 1, AP2 = "hello", B = be });
                be.BP2.Add(new AEntity() { AP1 = 2, AP2 = "two", B = be });

                var belist = new List<BEntity>();
                belist.Add(be);
                _bei = belist.AsQueryable();
            }

            [Fact]
            public void Should_not_throw_exception()
            {
                typeof(Exception).ShouldNotBeThrownBy(() => _bei.ProjectTo<B>(Configuration));
            }
        }
    }
}