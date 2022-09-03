namespace AutoMapper.UnitTests.MemberResolution;

public class When_multiple_source_members_match_postfix : AutoMapperSpecBase
{
    class Order
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

    class OrderDto
    {
        public int ProductId { get; set; }
    }
    class Product
    {
        public int Id { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.RecognizePostfixes("Id");
        c.CreateMap<Order, OrderDto>();
    });
    [Fact]
    public void Should_prefer_the_property()
    {
        var destination = Map<OrderDto>(new Order { ProductId = 12, Product = new() { Id = 42 } });
        destination.ProductId.ShouldBe(12);
    }
}
public class When_multiple_source_members_match : AutoMapperSpecBase
{
    class Source
    {
        public int Value { get; set; }
        public int GetValue() => 10;
        public int OtherValue { get; set; }
        public int GetOtherValue { get; set; }
    }
    class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateMap<Source, Destination>());
    [Fact]
    public void Should_prefer_the_property()
    {
        var destination = Map<Destination>(new Source { Value = 42, OtherValue = 42 });
        destination.Value.ShouldBe(42);
        destination.OtherValue.ShouldBe(42);
    }
}
public class When_mapping_derived_classes_in_arrays : AutoMapperSpecBase
{
    private DtoObject[] _result;

    public class ModelObject
    {
        public string BaseString { get; set; }
    }

    public class ModelSubObject : ModelObject
    {
        public string SubString { get; set; }
    }

    public class DtoObject
    {
        public string BaseString { get; set; }
    }

    public class DtoSubObject : DtoObject
    {
        public string SubString { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {

        cfg.CreateMap<ModelObject, DtoObject>()
            .Include<ModelSubObject, DtoSubObject>();

        cfg.CreateMap<ModelSubObject, DtoSubObject>();
    });

    protected override void Because_of()
    {
        var model = new[]
        {
            new ModelObject {BaseString = "Base1"},
            new ModelSubObject {BaseString = "Base2", SubString = "Sub2"}
        };
        _result = (DtoObject[]) Mapper.Map(model, typeof (ModelObject[]), typeof (DtoObject[]));
    }

    [Fact]
    public void Should_map_both_the_base_and_sub_objects()
    {
        _result.Length.ShouldBe(2);
        _result[0].BaseString.ShouldBe("Base1");
        _result[1].BaseString.ShouldBe("Base2");
    }

    [Fact]
    public void Should_map_to_the_correct_respective_dto_types()
    {
        _result[0].ShouldBeOfType(typeof (DtoObject));
        _result[1].ShouldBeOfType(typeof (DtoSubObject));
    }
}

public class When_mapping_derived_classes : AutoMapperSpecBase
{
    private DtoObject _result;

    public class ModelObject
    {
        public string BaseString { get; set; }
    }

    public class ModelSubObject : ModelObject
    {
        public string SubString { get; set; }
    }

    public class DtoObject
    {
        public string BaseString { get; set; }
    }

    public class DtoSubObject : DtoObject
    {
        public string SubString { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, DtoObject>()
            .Include<ModelSubObject, DtoSubObject>();

        cfg.CreateMap<ModelSubObject, DtoSubObject>();
    });

    protected override void Because_of()
    {
        var model = new ModelSubObject { BaseString = "Base2", SubString = "Sub2" };

        _result = Mapper.Map<ModelObject, DtoObject>(model);
    }

    [Fact]
    public void Should_map_to_the_correct_dto_types()
    {
        _result.ShouldBeOfType(typeof(DtoSubObject));
    }
}

public class When_mapping_derived_classes_from_intefaces_to_abstract : AutoMapperSpecBase
{
    private DtoObject[] _result;

    public interface IModelObject
    {
        string BaseString { get; set; }
    }

    public class ModelSubObject : IModelObject
    {
        public string SubString { get; set; }
        public string BaseString { get; set; }
    }

    public abstract class DtoObject
    {
        public virtual string BaseString { get; set; }
    }

    public class DtoSubObject : DtoObject
    {
        public string SubString { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<IModelObject, DtoObject>()
            .Include<ModelSubObject, DtoSubObject>();

        cfg.CreateMap<ModelSubObject, DtoSubObject>();
    });

    protected override void Because_of()
    {
        var model = new IModelObject[]
        {
            new ModelSubObject {BaseString = "Base2", SubString = "Sub2"}
        };
        _result = (DtoObject[]) Mapper.Map(model, typeof (IModelObject[]), typeof (DtoObject[]));
        base.Because_of();
    }

    [Fact]
    public void Should_map_both_the_base_and_sub_objects()
    {
        _result.Length.ShouldBe(1);
        _result[0].BaseString.ShouldBe("Base2");
    }

    [Fact]
    public void Should_map_to_the_correct_respective_dto_types()
    {
        _result[0].ShouldBeOfType(typeof (DtoSubObject));
        ((DtoSubObject) _result[0]).SubString.ShouldBe("Sub2");
    }
}

public class When_mapping_derived_classes_as_property_of_top_object : AutoMapperSpecBase
{
    private DtoModel _result;

    public class Model
    {
        public IModelObject Object { get; set; }
    }

    public interface IModelObject
    {
        string BaseString { get; set; }
    }

    public class ModelSubObject : IModelObject
    {
        public string SubString { get; set; }
        public string BaseString { get; set; }
    }

    public class DtoModel
    {
        public DtoObject Object { get; set; }
    }

    public abstract class DtoObject
    {
        public virtual string BaseString { get; set; }
    }

    public class DtoSubObject : DtoObject
    {
        public string SubString { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Model, DtoModel>();

        cfg.CreateMap<IModelObject, DtoObject>()
            .Include<ModelSubObject, DtoSubObject>();

        cfg.CreateMap<ModelSubObject, DtoSubObject>();
    });

    [Fact]
    public void Should_map_object_to_sub_object()
    {
        var model = new Model
            {
                Object = new ModelSubObject {BaseString = "Base2", SubString = "Sub2"}
            };

        _result = Mapper.Map<Model, DtoModel>(model);
        _result.Object.ShouldNotBeNull();
        _result.Object.ShouldBeOfType<DtoSubObject>();
        _result.Object.ShouldBeOfType<DtoSubObject>();
        _result.Object.BaseString.ShouldBe("Base2");
        ((DtoSubObject) _result.Object).SubString.ShouldBe("Sub2");
    }
}

public class When_mapping_dto_with_only_properties : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public DateTime BaseDate { get; set; }
        public ModelSubObject Sub { get; set; }
        public ModelSubObject Sub2 { get; set; }
        public ModelSubObject SubWithExtraName { get; set; }
        public ModelSubObject SubMissing { get; set; }
    }

    public class ModelSubObject
    {
        public string ProperName { get; set; }
        public ModelSubSubObject SubSub { get; set; }
    }

    public class ModelSubSubObject
    {
        public string IAmACoolProperty { get; set; }
    }

    public class ModelDto
    {
        public DateTime BaseDate { get; set; }
        public string SubProperName { get; set; }
        public string Sub2ProperName { get; set; }
        public string SubWithExtraNameProperName { get; set; }
        public string SubSubSubIAmACoolProperty { get; set; }
        public string SubMissingSubSubIAmACoolProperty { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {

        cfg.CreateMap<ModelObject, ModelDto>();
    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            BaseDate = new DateTime(2007, 4, 5),
            Sub = new ModelSubObject
            {
                ProperName = "Some name",
                SubSub = new ModelSubSubObject
                {
                    IAmACoolProperty = "Cool daddy-o"
                }
            },
            Sub2 = new ModelSubObject
            {
                ProperName = "Sub 2 name"
            },
            SubWithExtraName = new ModelSubObject
            {
                ProperName = "Some other name"
            },
            SubMissing = new ModelSubObject
            {
                ProperName = "I have a missing sub sub object"
            }
        };
        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_map_item_in_first_level_of_hierarchy()
    {
        _result.BaseDate.ShouldBe(new DateTime(2007, 4, 5));
    }

    [Fact]
    public void Should_map_a_member_with_a_number()
    {
        _result.Sub2ProperName.ShouldBe("Sub 2 name");
    }

    [Fact]
    public void Should_map_item_in_second_level_of_hierarchy()
    {
        _result.SubProperName.ShouldBe("Some name");
    }

    [Fact]
    public void Should_map_item_with_more_items_in_property_name()
    {
        _result.SubWithExtraNameProperName.ShouldBe("Some other name");
    }

    [Fact]
    public void Should_map_item_in_any_level_of_depth_in_the_hierarchy()
    {
        _result.SubSubSubIAmACoolProperty.ShouldBe("Cool daddy-o");
    }
}

public class When_mapping_dto_with_only_fields : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public DateTime BaseDate;
        public ModelSubObject Sub;
        public ModelSubObject Sub2;
        public ModelSubObject SubWithExtraName;
        public ModelSubObject SubMissing;
    }

    public class ModelSubObject
    {
        public string ProperName;
        public ModelSubSubObject SubSub;
    }

    public class ModelSubSubObject
    {
        public string IAmACoolProperty;
    }

    public class ModelDto
    {
        public DateTime BaseDate;
        public string SubProperName;
        public string Sub2ProperName;
        public string SubWithExtraNameProperName;
        public string SubSubSubIAmACoolProperty;
        public string SubMissingSubSubIAmACoolProperty;            
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            BaseDate = new DateTime(2007, 4, 5),
            Sub = new ModelSubObject
            {
                ProperName = "Some name",
                SubSub = new ModelSubSubObject
                {
                    IAmACoolProperty = "Cool daddy-o"
                }
            },
            Sub2 = new ModelSubObject
            {
                ProperName = "Sub 2 name"
            },
            SubWithExtraName = new ModelSubObject
            {
                ProperName = "Some other name"
            },
            SubMissing = new ModelSubObject
            {
                ProperName = "I have a missing sub sub object"
            }
        };
        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_map_item_in_first_level_of_hierarchy()
    {
        _result.BaseDate.ShouldBe(new DateTime(2007, 4, 5));
    }

    [Fact]
    public void Should_map_a_member_with_a_number()
    {
        _result.Sub2ProperName.ShouldBe("Sub 2 name");
    }

    [Fact]
    public void Should_map_item_in_second_level_of_hierarchy()
    {
        _result.SubProperName.ShouldBe("Some name");
    }

    [Fact]
    public void Should_map_item_with_more_items_in_property_name()
    {
        _result.SubWithExtraNameProperName.ShouldBe("Some other name");
    }

    [Fact]
    public void Should_map_item_in_any_level_of_depth_in_the_hierarchy()
    {
        _result.SubSubSubIAmACoolProperty.ShouldBe("Cool daddy-o");
    }
}

public class When_mapping_dto_with_fields_and_properties : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public DateTime BaseDate { get; set;}
        public ModelSubObject Sub;
        public ModelSubObject Sub2 { get; set;}
        public ModelSubObject SubWithExtraName;
        public ModelSubObject SubMissing { get; set; }
    }

    public class ModelSubObject
    {
        public string ProperName { get; set;}
        public ModelSubSubObject SubSub;
    }

    public class ModelSubSubObject
    {
        public string IAmACoolProperty { get; set;}
    }

    public class ModelDto
    {
        public DateTime BaseDate;
        public string SubProperName;
        public string Sub2ProperName { get; set;}
        public string SubWithExtraNameProperName;
        public string SubSubSubIAmACoolProperty;
        public string SubMissingSubSubIAmACoolProperty { get; set;}
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            BaseDate = new DateTime(2007, 4, 5),
            Sub = new ModelSubObject
            {
                ProperName = "Some name",
                SubSub = new ModelSubSubObject
                {
                    IAmACoolProperty = "Cool daddy-o"
                }
            },
            Sub2 = new ModelSubObject
            {
                ProperName = "Sub 2 name"
            },
            SubWithExtraName = new ModelSubObject
            {
                ProperName = "Some other name"
            },
            SubMissing = new ModelSubObject
            {
                ProperName = "I have a missing sub sub object"
            }
        };
        _result = Mapper.Map<ModelObject, ModelDto>(model);
        base.Because_of();
    }

    [Fact]
    public void Should_map_item_in_first_level_of_hierarchy()
    {
        _result.BaseDate.ShouldBe(new DateTime(2007, 4, 5));
    }

    [Fact]
    public void Should_map_a_member_with_a_number()
    {
        _result.Sub2ProperName.ShouldBe("Sub 2 name");
    }

    [Fact]
    public void Should_map_item_in_second_level_of_hierarchy()
    {
        _result.SubProperName.ShouldBe("Some name");
    }

    [Fact]
    public void Should_map_item_with_more_items_in_property_name()
    {
        _result.SubWithExtraNameProperName.ShouldBe("Some other name");
    }

    [Fact]
    public void Should_map_item_in_any_level_of_depth_in_the_hierarchy()
    {
        _result.SubSubSubIAmACoolProperty.ShouldBe("Cool daddy-o");
    }
}

public class When_ignoring_a_dto_property_during_configuration : AutoMapperSpecBase
{
    public class Source
    {
        public string Value { get; set; }
    }

    public class Destination
    {
        public bool Ignored
        {
            get { return true; }
        }

        public string Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(x => x.Ignored, opt => opt.Ignore());
    });


    [Fact]
    public void Should_not_report_it_as_unmapped()
    {
        var allTypeMaps = Configuration.GetAllTypeMaps();
        foreach (var typeMap in allTypeMaps)
        {
            typeMap.GetUnmappedPropertyNames().ShouldBeOfLength(0);
        }
    }

    [Fact]
    public void Should_map_successfully()
    {
        var destination = Mapper.Map<Source, Destination>(new Source {Value = "foo"});
        destination.Value.ShouldBe("foo");
        destination.Ignored.ShouldBeTrue();
    }
}

public class When_mapping_dto_with_get_methods : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public string GetSomeCoolValue()
        {
            return "Cool value";
        }

        public ModelSubObject Sub { get; set; }
    }

    public class ModelSubObject
    {
        public string GetSomeOtherCoolValue()
        {
            return "Even cooler";
        }
    }

    public class ModelDto
    {
        public string SomeCoolValue { get; set; }
        public string SubSomeOtherCoolValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            Sub = new ModelSubObject()
        };


        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_map_base_method_value()
    {
        _result.SomeCoolValue.ShouldBe("Cool value");
    }

    [Fact]
    public void Should_map_second_level_method_value_off_of_property()
    {
        _result.SubSomeOtherCoolValue.ShouldBe("Even cooler");
    }
}

public class When_mapping_a_dto_with_names_matching_properties : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public string SomeCoolValue()
        {
            return "Cool value";
        }

        public ModelSubObject Sub { get; set; }
    }

    public class ModelSubObject
    {
        public string SomeOtherCoolValue()
        {
            return "Even cooler";
        }
    }

    public class ModelDto
    {
        public string SomeCoolValue { get; set; }
        public string SubSomeOtherCoolValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            Sub = new ModelSubObject()
        };


        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_map_base_method_value()
    {
        _result.SomeCoolValue.ShouldBe("Cool value");
    }

    [Fact]
    public void Should_map_second_level_method_value_off_of_property()
    {
        _result.SubSomeOtherCoolValue.ShouldBe("Even cooler");
    }
}

public class When_mapping_with_a_dto_subtype : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public ModelSubObject Sub { get; set; }
    }

    public class ModelSubObject
    {
        public string SomeValue { get; set; }
    }

    public class ModelDto
    {
        public ModelSubDto Sub { get; set; }
    }

    public class ModelSubDto
    {
        public string SomeValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
        cfg.CreateMap<ModelSubObject, ModelSubDto>();

    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            Sub = new ModelSubObject
            {
                SomeValue = "Some value"
            }
        };

        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_map_the_model_sub_type_to_the_dto_sub_type()
    {
        _result.Sub.ShouldNotBeNull();
        _result.Sub.SomeValue.ShouldBe("Some value");
    }
}

public class When_mapping_a_dto_with_a_set_only_property_and_a_get_method : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelDto
    {
        public int SomeValue { get; set; }
    }

    public class ModelObject
    {
        private int _someValue;

        public int SomeValue
        {
            set { _someValue = value; }
        }

        public int GetSomeValue()
        {
            return _someValue;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();

    });

    protected override void Because_of()
    {
        var model = new ModelObject();
        model.SomeValue = 46;

        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_map_the_get_method_to_the_dto()
    {
        _result.SomeValue.ShouldBe(46);
    }
}

public class When_mapping_using_a_custom_member_mappings : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelObject
    {
        public int Blarg { get; set; }
        public string MoreBlarg { get; set; }

        public int SomeMethodToGetMoreBlarg()
        {
            return 45;
        }

        public string SomeValue { get; set; }
        public ModelSubObject SomeWeirdSubObject { get; set; }

        public string IAmSomeMethod()
        {
            return "I am some method";
        }
    }

    public class ModelSubObject
    {
        public int Narf { get; set; }
        public ModelSubSubObject SubSub { get; set; }

        public string SomeSubValue()
        {
            return "I am some sub value";
        }
    }

    public class ModelSubSubObject
    {
        public int Norf { get; set; }

        public string SomeSubSubValue()
        {
            return "I am some sub sub value";
        }
    }

    public class ModelDto
    {
        public int Splorg { get; set; }
        public string SomeValue { get; set; }
        public string SomeMethod { get; set; }
        public int SubNarf { get; set; }
        public string SubValue { get; set; }
        public int GrandChildInt { get; set; }
        public string GrandChildString { get; set; }
        public int BlargPlus3 { get; set; }
        public int BlargMinus2 { get; set; }
        public int MoreBlarg { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg
            .CreateMap<ModelObject, ModelDto>()
            .ForMember(dto => dto.Splorg, opt => opt.MapFrom(m => m.Blarg))
            .ForMember(dto => dto.SomeMethod, opt => opt.MapFrom(m => m.IAmSomeMethod()))
            .ForMember(dto => dto.SubNarf, opt => opt.MapFrom(m => m.SomeWeirdSubObject.Narf))
            .ForMember(dto => dto.SubValue, opt => opt.MapFrom(m => m.SomeWeirdSubObject.SomeSubValue()))
            .ForMember(dto => dto.GrandChildInt, opt => opt.MapFrom(m => m.SomeWeirdSubObject.SubSub.Norf))
            .ForMember(dto => dto.GrandChildString,
                opt => opt.MapFrom(m => m.SomeWeirdSubObject.SubSub.SomeSubSubValue()))
            .ForMember(dto => dto.MoreBlarg, opt => opt.MapFrom(m => m.SomeMethodToGetMoreBlarg()))
            .ForMember(dto => dto.BlargPlus3, opt => opt.MapFrom(m => m.Blarg.Plus(3)))
            .ForMember(dto => dto.BlargMinus2, opt => opt.MapFrom(m => m.Blarg - 2));

    });

    protected override void Because_of()
    {
        var model = new ModelObject
        {
            Blarg = 10,
            SomeValue = "Some value",
            SomeWeirdSubObject = new ModelSubObject
            {
                Narf = 5,
                SubSub = new ModelSubSubObject
                {
                    Norf = 15
                }
            },
            MoreBlarg = "adsfdsaf"
        };
        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_preserve_the_existing_mapping()
    {
        _result.SomeValue.ShouldBe("Some value");
    }

    [Fact]
    public void Should_map_top_level_properties()
    {
        _result.Splorg.ShouldBe(10);
    }

    [Fact]
    public void Should_map_methods_results()
    {
        _result.SomeMethod.ShouldBe("I am some method");
    }

    [Fact]
    public void Should_map_children_properties()
    {
        _result.SubNarf.ShouldBe(5);
    }

    [Fact]
    public void Should_map_children_methods()
    {
        _result.SubValue.ShouldBe("I am some sub value");
    }

    [Fact]
    public void Should_map_grandchildren_properties()
    {
        _result.GrandChildInt.ShouldBe(15);
    }

    [Fact]
    public void Should_map_grandchildren_methods()
    {
        _result.GrandChildString.ShouldBe("I am some sub sub value");
    }

    [Fact]
    public void Should_map_blarg_plus_three_using_extension_method()
    {
        _result.BlargPlus3.ShouldBe(13);
    }

    [Fact]
    public void Should_map_blarg_minus_2_using_lambda()
    {
        _result.BlargMinus2.ShouldBe(8);
    }

    [Fact]
    public void Should_override_existing_matches_for_new_mappings()
    {
        _result.MoreBlarg.ShouldBe(45);
    }
}

public class When_mapping_using_custom_member_mappings_without_generics : AutoMapperSpecBase
{
    private OrderDTO _result;

    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Customer { get; set; }
        public string ShippingCode { get; set; }
        public string Zip { get; set; }
    }

    public class OrderDTO
    {
        public int Id { get; set; }
        public string CurrentState { get; set; }
        public string Contact { get; set; }
        public string Tracking { get; set; }
        public string Postal { get; set; }
    }

    public class StringCAPS : IMemberValueResolver<object, object, string, string>
    {
        public string Resolve(object s, object d, string source, string dest, ResolutionContext context)
        {
            return source.ToUpper();
        }
    }

    public class StringLower : IMemberValueResolver<object, object, string, string>
    {
        public string Resolve(object s, object d, string source, string dest, ResolutionContext context)
        {
            return source.ToLower();
        }
    }

    public class StringPadder : IMemberValueResolver<object, object, string, string>
    {
        private readonly int _desiredLength;

        public StringPadder(int desiredLength)
        {
            _desiredLength = desiredLength;
        }

        public string Resolve(object s, object d, string source, string dest, ResolutionContext context)
        {
            return source.PadLeft(_desiredLength);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof (Order), typeof (OrderDTO))
            .ForMember("CurrentState", map => map.MapFrom("Status"))
            .ForMember("Contact", map => map.MapFrom(new StringCAPS(), "Customer"))
            .ForMember("Tracking", map => map.MapFrom(typeof (StringLower), "ShippingCode"))
            .ForMember("Postal", map => map.MapFrom(new StringPadder(6), "Zip"));

    });

    protected override void Because_of()
    {
            var order = new Order
        {
            Id = 7,
            Status = "Pending",
            Customer = "Buster",
            ShippingCode = "AbcxY23",
            Zip = "XYZ"
        };
        _result = Mapper.Map<Order, OrderDTO>(order);
    }

    [Fact]
    public void Should_preserve_existing_mapping()
    {
        _result.Id.ShouldBe(7);
    }

    [Fact]
    public void Should_support_custom_source_member()
    {
        _result.CurrentState.ShouldBe("Pending");
    }

    [Fact]
    public void Should_support_custom_resolver_on_custom_source_member()
    {
        _result.Contact.ShouldBe("BUSTER");
    }

    [Fact]
    public void Should_support_custom_resolver_by_type_on_custom_source_member()
    {
        _result.Tracking.ShouldBe("abcxy23");
    }

    [Fact]
    public void Should_support_custom_resolver_by_generic_type_with_constructor_on_custom_source_member()
    {
        _result.Postal.ShouldBe("   XYZ");
    }

}
    
public class When_mapping_to_a_top_level_camelCased_destination_member : AutoMapperSpecBase
{
    private Destination _result;

    public class Source
    {
        public int SomeValueWithPascalName { get; set; }
    }

    public class Destination
    {
        public int someValueWithPascalName { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source {SomeValueWithPascalName = 5};
        _result = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_match_to_PascalCased_source_member()
    {
        _result.someValueWithPascalName.ShouldBe(5);
    }
}

public class When_mapping_to_a_self_referential_object : AutoMapperSpecBase
{
    private CategoryDto _result;

    public class Category
    {
        public string Name { get; set; }
        public IList<Category> Children { get; set; }
    }

    public class CategoryDto
    {
        public string Name { get; set; }
        public CategoryDto[] Children { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Category, CategoryDto>();
    });

    protected override void Because_of()
    {
        var category = new Category
        {
            Name = "Grandparent",
            Children = new List<Category>()
            {
                new Category { Name = "Parent 1", Children = new List<Category>()
                {
                    new Category { Name = "Child 1"},
                    new Category { Name = "Child 2"},
                    new Category { Name = "Child 3"},
                }},
                new Category { Name = "Parent 2", Children = new List<Category>()
                {
                    new Category { Name = "Child 4"},
                    new Category { Name = "Child 5"},
                    new Category { Name = "Child 6"},
                    new Category { Name = "Child 7"},
                }},
            }
        };
        _result = Mapper.Map<Category, CategoryDto>(category);
    }

    [Fact]
    public void Should_resolve_any_level_of_hierarchies()
    {
        _result.Name.ShouldBe("Grandparent");
        _result.Children.Length.ShouldBe(2);
        _result.Children[0].Children.Length.ShouldBe(3);
        _result.Children[1].Children.Length.ShouldBe(4);
    }
}
    
public class When_mapping_to_types_in_a_non_generic_manner : AutoMapperSpecBase
{
    private Destination _result;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof (Source), typeof (Destination));
    });

    protected override void Because_of()
    {
        _result = Mapper.Map<Source, Destination>(new Source {Value = 5});
    }

    [Fact]
    public void Should_allow_for_basic_mapping()
    {
        _result.Value.ShouldBe(5);
    }
}

public class When_matching_source_and_destination_members_with_underscored_members : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public SubSource some_source { get; set; }
    }

    public class SubSource
    {
        public int value { get; set; }
    }

    public class Destination
    {
        public int some_source_value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
        cfg.DestinationMemberNamingConvention = new LowerUnderscoreNamingConvention();
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {some_source = new SubSource {value = 8}});
    }

    [Fact]
    public void Should_use_underscores_as_tokenizers_to_flatten()
    {
        _destination.some_source_value.ShouldBe(8);
    }
}

public class When_source_members_configured_in_a_root_profile_contain_prefixes : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int FooValue { get; set; }
        public int GetOtherValue()
        {
            return 10;
        }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }

    public class ChildProfile : Profile
    {
        public ChildProfile()
        {
            CreateMap<Source, Destination>();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePrefixes("Foo");
        cfg.AddProfile<ChildProfile>();                
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { FooValue = 5 });
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.Value.ShouldBe(5);
    }

    [Fact]
    public void Default_prefix_included()
    {
        _destination.OtherValue.ShouldBe(10);
    }
}

public class When_source_members_contain_prefixes : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int FooValue { get; set; }
        public int GetOtherValue() 
        {
            return 10; 
        }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePrefixes("Foo");
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {FooValue = 5});
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.Value.ShouldBe(5);
    }

    [Fact]
    public void Default_prefix_included()
    {
        _destination.OtherValue.ShouldBe(10);
    }
}


public class When_source_members_contain_prefixes_with_lowercase : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int fooValue { get; set; }
        public int GetOtherValue()
        {
            return 10;
        }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePrefixes("Foo");
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { fooValue = 5 });
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.Value.ShouldBe(5);
    }

    [Fact]
    public void Default_prefix_included()
    {
        _destination.OtherValue.ShouldBe(10);
    }
}

public class When_source_members_contain_postfixes_with_lowercase : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Valuefoo { get; set; }
        public int GetOtherValue()
        {
            return 10;
        }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePostfixes("Foo");
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Valuefoo = 5 });
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.Value.ShouldBe(5);
    }

    [Fact]
    public void Default_prefix_included()
    {
        _destination.OtherValue.ShouldBe(10);
    }
}

public class When_source_members_configured_in_a_root_profile_contain_postfixes_and_prefixes : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int FooValueBar { get; set; }
        public int GetOtherValue()
        {
            return 10;
        }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }

    public class ChildProfile : Profile
    {
        public ChildProfile()
        {
            CreateMap<Source, Destination>();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePrefixes("Foo");
        cfg.RecognizePostfixes("Bar");
        cfg.AddProfile<ChildProfile>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { FooValueBar = 5 });
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.Value.ShouldBe(5);
    }

    [Fact]
    public void Default_prefix_included()
    {
        _destination.OtherValue.ShouldBe(10);
    }
}


public class When_source_members_contain_postfixes_and_prefixes : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int FooValueBar { get; set; }
        public int GetOtherValue()
        {
            return 10;
        }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int OtherValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizePrefixes("Foo");
        cfg.RecognizePostfixes("Bar");
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { FooValueBar = 5 });
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.Value.ShouldBe(5);
    }

    [Fact]
    public void Default_prefix_included()
    {
        _destination.OtherValue.ShouldBe(10);
    }
}

public class When_source_member_names_match_with_underscores : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int I_amaCraAZZEE____Name { get; set; }
    }

    public class Destination
    {
        public int I_amaCraAZZEE____Name { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {I_amaCraAZZEE____Name = 5});
    }

    [Fact]
    public void Should_match_based_on_name()
    {
        _destination.I_amaCraAZZEE____Name.ShouldBe(5);
    }
}

public class When_recognizing_explicit_member_aliases : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Foo { get; set; }
    }

    public class Destination
    {
        public int Bar { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ReplaceMemberName("Foo", "Bar");
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {Foo = 5});
    }

    [Fact]
    public void Members_that_match_alias_should_be_matched()
    {
        _destination.Bar.ShouldBe(5);
    }
}

public class When_destination_members_contain_prefixes : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Value { get; set; }
        public int Value2 { get; set; }
    }

    public class Destination
    {
        public int FooValue { get; set; }
        public int BarValue2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.RecognizeDestinationPrefixes("Foo", "Bar");
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Value = 5, Value2 = 10 });
    }

    [Fact]
    public void Registered_prefixes_ignored()
    {
        _destination.FooValue.ShouldBe(5);
        _destination.BarValue2.ShouldBe(10);
    }
}

public class When_destination_type_has_private_members : AutoMapperSpecBase
{
    private IDestination _destination;

    public class Source
    {
        public int Value { get; set; }
    }

    public interface IDestination
    {
        int Value { get; }
    }

    public class Destination : IDestination
    {
        public Destination(int value)
        {
            Value = value;
        }

        private Destination()
        {
        }

        public int Value { get; private set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {Value = 5});
    }

    [Fact]
    public void Should_use_private_accessors_and_constructors()
    {
        _destination.Value.ShouldBe(5);
    }
}

public static class MapFromExtensions
{
    public static int Plus(this int left, int right)
    {
        return left + right;
    }
}