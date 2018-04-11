using AutoMapper.QueryableExtensions;
using AutoMapper.XpressionMapper.Extensions;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class CustomStructTests
    {
        [Fact]
        public void Project_from_custom_struct()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<CustomMaster, MasterDto>()
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.Value))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<CustomMaster> {
                new CustomMaster { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<MasterDto, CustomMaster>(mapper, s => s.Name == "Harry Marry");
            var output2 = source.AsQueryable().Query<MasterDto, CustomMaster>(mapper, s => s.Where(c => c.Name == "Harry Marry"));
            var masterDtoQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<MasterDto>()
                .Where(d => d.Name == "Harry Marry")
                .ToList();

            // Assert
            output1.First().Name.ShouldBe("Harry Marry");
            (Enumerable.First(output2).Name as string).ShouldBe("Harry Marry");
            masterDtoQuery.First().Name.ShouldBe("Harry Marry");
        }

        [Fact]
        public void Project_from_custom_struct_with_projection()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<CustomMaster, MasterDto>()
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.Value))
                    .ReverseMap();

            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<CustomMaster> {
                new CustomMaster { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<MasterDto, CustomMaster>(mapper, s => s.Name == "Harry Marry").Select(c => new { c.Name });
            var output2 = source.AsQueryable().Query<MasterDto, CustomMaster>(mapper, s => s.Where(c => c.Name == "Harry Marry").Select(c => new { c.Name }));
            var masterDtoProjectedQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<MasterDto>()
                .Where(d => d.Name == "Harry Marry")
                .Select(x => new
                {
                    x.Name
                })
                .ToList();

            // Assert
            output1.First().Name.ShouldBe("Harry Marry");
            (Enumerable.First(output2).Name as string).ShouldBe("Harry Marry");
            masterDtoProjectedQuery.First().Name.ShouldBe("Harry Marry");
        }

        [Fact]
        public void Project_from_custom_struct_with_custom_mapping()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<CustomMaster, MasterDto>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name + " !!!"))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<CustomMaster> {
                new CustomMaster { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<MasterDto, CustomMaster>(mapper, s => s.Name == "Harry Marry !!!");
            var output2 = source.AsQueryable().Query<MasterDto, CustomMaster>(mapper, s => s.Where(c => c.Name == "Harry Marry !!!"));
            var masterDtoQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<MasterDto>()
                .Where(d => d.Name == "Harry Marry !!!")
                .ToList();

            // Assert
            output1.First().Name.ShouldBe("Harry Marry !!!");
            (Enumerable.First(output2).Name as string).ShouldBe("Harry Marry !!!");
            masterDtoQuery.First().Name.ShouldBe("Harry Marry !!!");
        }

        [Fact]
        public void Project_from_custom_struct_with_custom_mapping_and_projection()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<CustomMaster, MasterDto>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name + " !!!"))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<CustomMaster> {
                new CustomMaster { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<MasterDto, CustomMaster>(mapper, s => s.Name == "Harry Marry !!!").Select(c => new { c.Name });
            var output2 = source.AsQueryable().Query<MasterDto, CustomMaster>(mapper, s => s.Where(c => c.Name == "Harry Marry !!!").Select(c => new { c.Name }));
            var masterDtoProjectedQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<MasterDto>()
                .Where(d => d.Name == "Harry Marry !!!")
                .Select(x => new
                {
                    x.Name
                })
                .ToList();

            // Assert
            output1.First().Name.ShouldBe("Harry Marry !!!");
            (Enumerable.First(output2).Name as string).ShouldBe("Harry Marry !!!");
            masterDtoProjectedQuery.First().Name.ShouldBe("Harry Marry !!!");
        }

        [Fact]
        public void Project_from_custom_struct_with_custom_mapping_and_binding()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<CustomMaster, MasterDto>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name + " !!!"))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<CustomMaster> {
                new CustomMaster { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<MasterDto, CustomMaster>(mapper, s => s.Name == "Harry Marry !!!").Select(x => new MasterDto1 { Id = x.Id, Name = x.Name });
            var output2 = source.AsQueryable().Query<MasterDto, CustomMaster>(mapper, s => s.Where(c => c.Name == "Harry Marry !!!").Select(x => new MasterDto1 { Id = x.Id, Name = x.Name }));
            var masterDtoProjectedQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<MasterDto>()
                .Where(d => d.Name == "Harry Marry !!!")
                .Select(x => new MasterDto1
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();

            // Assert
            output1.First().Name.ShouldBe("Harry Marry !!!");
            (Enumerable.First(output2).Name as string).ShouldBe("Harry Marry !!!");
            masterDtoProjectedQuery.First().Name.ToString().ShouldBe("Harry Marry !!!");
        }


        [Fact]
        public void Project_to_custom_struct()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MasterDto, CustomMaster>()
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => new CustomString(s.Name)))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<MasterDto> {
                new MasterDto { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<CustomMaster, MasterDto>(mapper, s => s.Name == "Harry Marry");
            var output2 = source.AsQueryable().Query<CustomMaster, MasterDto>(mapper, s => s.Where(c => c.Name == "Harry Marry"));
            var customMasterQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<CustomMaster>()
                .Where(d => d.Name == "Harry Marry")
                .ToList();

            // Assert
            output1.First().Name.ToString().ShouldBe("Harry Marry");
            ((CustomString)Enumerable.First(output2).Name).ToString().ShouldBe("Harry Marry");
            customMasterQuery.First().Name.ToString().ShouldBe("Harry Marry");
        }

        [Fact]
        public void Project_to_custom_struct_with_projection()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MasterDto, CustomMaster>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => new CustomString(src.Name + " !!!")))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<MasterDto> {
                new MasterDto { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<CustomMaster, MasterDto>(mapper, s => s.Name == "Harry Marry !!!").Select(c => new { c.Name });
            var output2 = source.AsQueryable().Query<CustomMaster, MasterDto>(mapper, s => s.Where(c => c.Name == "Harry Marry !!!").Select(c => new { c.Name }));
            var customMasterQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<CustomMaster>()
                .Where(d => d.Name == "Harry Marry !!!")
                .Select(x => new
                {
                    x.Name
                })
                .ToList();

            // Assert
            output1.First().Name.ToString().ShouldBe("Harry Marry !!!");
            ((CustomString)Enumerable.First(output2).Name).ToString().ShouldBe("Harry Marry !!!");
            customMasterQuery.First().Name.ToString().ShouldBe("Harry Marry !!!");
        }

        [Fact]
        public void Project_to_custom_struct_with_custom_mapping()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MasterDto, CustomMaster>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => new CustomString(src.Name + " !!!")))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<MasterDto> {
                new MasterDto { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<CustomMaster, MasterDto>(mapper, s => s.Name == "Harry Marry !!!");
            var output2 = source.AsQueryable().Query<CustomMaster, MasterDto>(mapper, s => s.Where(c => c.Name == "Harry Marry !!!"));
            var customMasterQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<CustomMaster>()
                .Where(d => d.Name == "Harry Marry !!!")
                .ToList();

            // Assert
            output1.First().Name.ToString().ShouldBe("Harry Marry !!!");
            ((CustomString)Enumerable.First(output2).Name).ToString().ShouldBe("Harry Marry !!!");
            customMasterQuery.First().Name.ToString().ShouldBe("Harry Marry !!!");
        }

        [Fact]
        public void Project_to_custom_struct_with_custom_mapping_and_projection()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MasterDto, CustomMaster>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => new CustomString(src.Name + " !!!")))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<MasterDto> {
                new MasterDto { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            // Act
            var output1 = source.AsQueryable().GetItems<CustomMaster, MasterDto>(mapper, s => s.Name == "Harry Marry !!!").Select(c => new { c.Name });
            var output2 = source.AsQueryable().Query<CustomMaster, MasterDto>(mapper, s => s.Where(c => c.Name == "Harry Marry !!!").Select(c => new { c.Name }));
            var customMasterQuery = source.AsQueryable().UseAsDataSource(mapper)
                .For<CustomMaster>()
                .Where(d => d.Name == "Harry Marry !!!")
                .Select(x => new
                {
                    x.Name
                })
                .ToList();

            // Assert
            output1.First().Name.ToString().ShouldBe("Harry Marry !!!");
            ((CustomString)Enumerable.First(output2).Name).ToString().ShouldBe("Harry Marry !!!");
            customMasterQuery.First().Name.ToString().ShouldBe("Harry Marry !!!");
        }

        [Fact]
        public void Change_return_type_from_custom_struct()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MasterDto, CustomMaster>()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => new CustomString(src.Name + " !!!")))
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<MasterDto> {
                new MasterDto { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            var source2 = source.AsQueryable().UseAsDataSource(mapper).For<CustomMaster>();

            // Act
            Expression<Func<CustomMaster, CustomString>> selection = s => s.Name;
            Expression<Func<MasterDto, string>> selectionMapped = mapper.MapExpression<Expression<Func<MasterDto, string>>>(selection);
            var output = source.Select(selectionMapped.Compile()).ToList();
            var output1 = source.AsQueryable().Query<CustomMaster, MasterDto>(mapper, s => s.Select(c => c.Name));
            var output2 = source2.Select(x => x.Name).ToList();

            // Assert
            output.First().ShouldBe("Harry Marry !!!");
            ((CustomString)Enumerable.First(output1)).Value.ShouldBe("Harry Marry !!!");
            output2.First().ToString().ShouldBe("Harry Marry !!!");
        }

        [Fact]
        public void Change_return_type_to_custom_struct()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MasterDto, CustomMaster>()
                    //.ForMember(dst => dst.Name, opt => opt.MapFrom(src => new CustomString(src.Name + " !!!")))
                    .ReverseMap()
                    .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name.Value + " !!!"));
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            var source = new List<CustomMaster> {
                new CustomMaster { Id = Guid.NewGuid(), Name = "Harry Marry" }
            };

            var source2 = source.AsQueryable().UseAsDataSource(mapper).For<MasterDto>();

            // Act
            Expression<Func<MasterDto, string>> selection = s => s.Name;
            Expression<Func<CustomMaster, CustomString>> selectionMapped = mapper.MapExpression<Expression<Func<CustomMaster, CustomString>>>(selection);
            var output = source.Select(selectionMapped.Compile()).ToList();
            var output1 = source.AsQueryable().Query<MasterDto, CustomMaster>(mapper, s => s.Select(c => c.Name));
            var output2 = source2.Select(x => x.Name).ToList();

            // Assert
            output.First().Value.ShouldBe("Harry Marry !!!");
            (Enumerable.First(output1) as string).ShouldBe("Harry Marry !!!");
            output2.First().ToString().ShouldBe("Harry Marry !!!");
        }
    }

    public class MasterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class MasterDto1
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomMaster
    {
        public Guid Id { get; set; }
        public CustomString Name { get; set; }
    }

    public struct CustomString : IComparable, IConvertible
    {
        private string value;
        private bool isNull;

        public CustomString(string value)
        {
            this.isNull = string.IsNullOrEmpty(value);
            this.value = value ?? string.Empty;
        }

        public string Value
        {
            get
            {
                if (IsNull || value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                if (Value == value && !IsNull)
                    return;

                if (string.IsNullOrEmpty(value) && !isNull)
                {
                    isNull = true;
                }

                this.value = value ?? string.Empty;
            }
        }

        public bool IsNull
        {
            get
            {
                return isNull;
            }
            set
            {
                if (isNull == value) return;
                isNull = value;
            }
        }

        public static implicit operator CustomString(string a)
        {
            return new CustomString(a);
        }

        public static implicit operator string(CustomString a)
        {
            return a.Value;
        }

        public static bool operator ==(CustomString a, CustomString b)
        {
            return a.Value == b.Value;
        }

        public static bool operator ==(CustomString a, string b)
        {
            return a.Value == b;
        }

        public static bool operator ==(string a, CustomString b)
        {
            return a == b.Value;
        }

        public static bool operator !=(CustomString a, CustomString b)
        {
            return a.Value != b.Value;
        }

        public static bool operator !=(CustomString a, string b)
        {
            return a.Value != b;
        }

        public static bool operator !=(string a, CustomString b)
        {
            return a != b.Value;
        }

        public static bool operator <(CustomString a, CustomString b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(CustomString a, CustomString b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <=(CustomString a, CustomString b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator >=(CustomString a, CustomString b)
        {
            return a.CompareTo(b) >= 0;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomString)
                return this == (CustomString)obj;
            if (obj is string)
                return this == (CustomString)((string)obj);
            return false;
        }


        public int CompareTo(object obj)
        {
            if (!(obj is CustomString))
                return -1;
            return CompareTo((CustomString)obj);
        }

        public TypeCode GetTypeCode()
        {
            if (this.IsNull)
                return TypeCode.String;
            return this.Value.GetTypeCode();
        }

        public bool EqualsValue(string obj)
        {
            return this.Equals((object)obj);
        }

        public override string ToString()
        {
            if (IsNull)
                return string.Empty;
            return Value;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToByte(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToSingle(provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)this.Value).ToUInt64(provider);
        }
    }

    static class Extensions
    {
        internal static ICollection<TModel> GetItems<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
        Expression<Func<TModel, bool>> filter = null,
        Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> queryableFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            if (filter != null)
                query = query.Where(f);

            //Call the store
            ICollection<TData> list = queryableFunc != null ? queryableFunc(query).ToList() : query.ToList();

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(list).ToList();
        }

        internal static dynamic Query<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, dynamic>> queryFunc = null) where TData : class
        {
            //Map the expressions
            Func<IQueryable<TData>, dynamic> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, dynamic>>>(queryFunc)?.Compile();

            //execute the query
            dynamic returnValue = mappedQueryFunc(query);
            Type returnType = returnValue.GetType();

            object result = returnType.GetInterfaces().Any(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryable<>))
                ? Enumerable.ToList(returnValue)
                : returnValue;

            IEnumerable<TData> tDataList = result as IEnumerable<TData>;
            TData tData = result as TData;

            //Map and return the data
            return tDataList != null
                ? mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(tDataList).ToList()
                : tData != null
                         ? mapper.Map<TData, TModel>(tData)
                         : result;
        }
    }
}
