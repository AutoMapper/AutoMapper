namespace AutoMapper.UnitTests;

public class CyclesWithInheritance : AutoMapperSpecBase
{
    class FlowChart
    {
        public FlowNode[] Nodes;
    }
    class FlowNode
    {
    }
    class FlowStep : FlowNode
    {
        public FlowNode Next;
    }
    class FlowDecision : FlowNode
    {
        public FlowNode True;
        public FlowNode False;
    }
    class FlowSwitch<T> : FlowNode
    {
        public IDictionary<T, object> Connections;
    }
    class FlowChartModel
    {
        public FlowNodeModel[] Nodes;
    }
    class FlowNodeModel
    {
        public Connection[] Connections;
    }
    class Connection
    {
        public FlowNodeModel Node;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<FlowChart, FlowChartModel>();
        cfg.CreateMap<FlowNode, FlowNodeModel>()
            .Include<FlowStep, FlowNodeModel>()
            .Include<FlowDecision, FlowNodeModel>()
            .Include(typeof(FlowSwitch<>), typeof(FlowNodeModel))
            .ForMember(d=>d.Connections, o=>o.Ignore());
        cfg.CreateMap<FlowStep, FlowNodeModel>().ForMember(d => d.Connections, o => o.MapFrom(s => new[] { s.Next }));
        cfg.CreateMap<FlowDecision, FlowNodeModel>().ForMember(d => d.Connections, o => o.MapFrom(s => new[] { s.True, s.False }));
        cfg.CreateMap(typeof(FlowSwitch<>), typeof(FlowNodeModel));
        cfg.CreateMap<FlowNode, Connection>().ForMember(d => d.Node, o => o.MapFrom(s => s));
        cfg.CreateMap(typeof(KeyValuePair<,>), typeof(Connection)).ForMember("Node", o => o.MapFrom("Key"));
    });
    [Fact]
    public void Should_map_ok()
    {
        var flowStep = new FlowStep();
        var flowDecision = new FlowDecision { False = flowStep, True = flowStep };
        flowStep.Next = flowDecision;
        var source = new FlowChart { Nodes = new FlowNode[] { flowStep, flowDecision } };
        var dest = Map<FlowChartModel>(source);
    }
}
public class When_the_source_has_cyclical_references_with_dynamic_map : AutoMapperSpecBase
{
    public class CDataTypeModel<T>
    {
        public string Name { get; set; }
        public List<CFieldDefinitionModel<T>> FieldDefinitionList { get; set; }
    }
    public class CDataTypeDTO<T>
    {
        public string Name { get; set; }
        public List<CFieldDefinitionDTO<T>> FieldDefinitionList { get; set; }
    }
    public class CFieldDefinitionModel<T>
    {
        public string Name { get; set; }
        public CDataTypeModel<T> DataType { get; set; }
        public CComponentDefinitionModel<T> ComponentDefinition { get; set; }
    }
    public class CFieldDefinitionDTO<T>
    {
        public string Name { get; set; }
        public CDataTypeDTO<T> DataType { get; set; }
        public CComponentDefinitionDTO<T> ComponentDefinition { get; set; }
    }
    public class CComponentDefinitionModel<T>
    {
        public string Name { get; set; }
        public List<CFieldDefinitionModel<T>> FieldDefinitionList { get; set; }
    }
    public class CComponentDefinitionDTO<T>
    {
        public string Name { get; set; }
        public List<CFieldDefinitionDTO<T>> FieldDefinitionList { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(CDataTypeModel<>), typeof(CDataTypeDTO<>)).ReverseMap();
        cfg.CreateMap(typeof(CFieldDefinitionModel<>), typeof(CFieldDefinitionDTO<>)).ReverseMap();
        cfg.CreateMap(typeof(CComponentDefinitionModel<>), typeof(CComponentDefinitionDTO<>)).ReverseMap();
    });

    [Fact]
    public void Should_map_ok()
    {
        var component = new CComponentDefinitionDTO<int>();
        var type = new CDataTypeDTO<int>();
        var field = new CFieldDefinitionDTO<int> { ComponentDefinition = component, DataType = type };
        type.FieldDefinitionList = component.FieldDefinitionList = new List<CFieldDefinitionDTO<int>> { field };
        var fieldModel = Mapper.Map<CFieldDefinitionModel<int>>(field);
        fieldModel.ShouldBeSameAs(fieldModel.ComponentDefinition.FieldDefinitionList[0]);
        fieldModel.ShouldBeSameAs(fieldModel.DataType.FieldDefinitionList[0]);
    }
}

public class When_the_same_map_is_used_again : AutoMapperSpecBase
{
    class Source
    {
        public InnerSource Inner;
        public OtherInnerSource OtherInner;
        public Item Value1;
        public Item Value2;
    }

    class InnerSource
    {
        public Item Value;
    }

    class OtherInnerSource
    {
        public Item Value;
    }

    class InnerDestination
    {
        public Item Value;
    }

    class OtherInnerDestination
    {
        public Item Value;
    }

    class Destination
    {
        public InnerDestination Inner;
        public OtherInnerDestination OtherInner;
        public Item Value1;
        public Item Value2;
    }

    class Item
    {
        public int Value;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<InnerSource, InnerDestination>();
        cfg.CreateMap<OtherInnerSource, OtherInnerDestination>();
        cfg.CreateMap<Item, Item>();
    });

    [Fact]
    public void Should_not_set_preserve_references()
    {
        Configuration.ResolveTypeMap(typeof(Item), typeof(Item)).PreserveReferences.ShouldBeFalse();
    }
}

public class When_the_source_has_cyclical_references : AutoMapperSpecBase
{
    public class Article
    {
        public int Id { get; set; }

        public virtual Supplier Supplier { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }

        public virtual ICollection<Supplier> Suppliers { get; set; }
    }

    public class ArticleViewModel
    {
        public int Id { get; set; }

        public SupplierViewModel Supplier { get; set; }
    }

    public class SupplierViewModel
    {
        public int Id { get; set; }

        public List<ContactViewModel> Contacts { get; set; }

    }

    public class ContactViewModel
    {
        public int Id { get; set; }

        public List<SupplierViewModel> Suppliers { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Article, ArticleViewModel>();
        cfg.CreateMap<Supplier, SupplierViewModel>();
        cfg.CreateMap<Contact, ContactViewModel>();
    });

    [Fact]
    public void Should_map_ok()
    {
        var article = new Article { Supplier = new Supplier() };
        article.Supplier.Contacts = new List<Contact> { new Contact { Suppliers = new List<Supplier> { article.Supplier } } };
        var supplier = Mapper.Map<ArticleViewModel>(article).Supplier;
        supplier.ShouldBe(supplier.Contacts[0].Suppliers[0]);
    }
}

public class When_the_source_has_cyclical_references_with_ForPath : AutoMapperSpecBase
{
    public class Article
    {
        public int Id { get; set; }

        public virtual Supplier Supplier { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }

        public virtual ICollection<Supplier> Suppliers { get; set; }
    }

    public class ArticleViewModel
    {
        public int Id { get; set; }

        public SupplierViewModel Supplier { get; set; }
    }

    public class SupplierViewModel
    {
        public int Id { get; set; }

        public List<ContactViewModel> Contacts { get; set; }

    }

    public class ContactViewModel
    {
        public int Id { get; set; }

        public List<SupplierViewModel> Suppliers1 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Article, ArticleViewModel>();
        cfg.CreateMap<Supplier, SupplierViewModel>();
        cfg.CreateMap<Contact, ContactViewModel>().ForPath(d=>d.Suppliers1, o=>o.MapFrom(s=>s.Suppliers));
    });

    [Fact]
    public void Should_map_ok()
    {
        var article = new Article { Supplier = new Supplier() };
        article.Supplier.Contacts = new List<Contact> { new Contact { Suppliers = new List<Supplier> { article.Supplier } } };
        var supplier = Mapper.Map<ArticleViewModel>(article).Supplier;
        supplier.ShouldBe(supplier.Contacts[0].Suppliers1[0]);
    }
}

public class When_the_source_has_cyclical_references_with_ignored_ForPath : AutoMapperSpecBase
{
    public class Supplier
    {
        public int Id { get; set; }

        public virtual Contact Contact { get; set; }
    }

    public class Contact
    {
        public int Id { get; set; }

        public Supplier Supplier { get; set; }
    }

    public class SupplierViewModel
    {
        public int Id { get; set; }

        public ContactViewModel Contact { get; set; }

    }

    public class ContactViewModel
    {
        public int Id { get; set; }

        public SupplierViewModel Supplier1 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Supplier, SupplierViewModel>().ForPath(d=>d.Contact.Supplier1, o=>
        {
            o.MapFrom(s => s.Contact.Supplier);
            o.Ignore();
        });
    });

    [Fact]
    public void Should_map_ok()
    {
        var supplier = new Supplier();
        supplier.Contact = new Contact { Supplier = supplier };
        Mapper.Map<SupplierViewModel>(supplier);
        Configuration.GetAllTypeMaps().All(tm => tm.PreserveReferences).ShouldBeFalse();
    }
}

public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship : AutoMapperSpecBase
{
    private ParentDto _dto;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ParentModel, ParentDto>();
        cfg.CreateMap<ChildModel, ChildDto>();
    });

    protected override void Because_of()
    {
        var parent = new ParentModel { ID = "PARENT_ONE" };

        parent.AddChild(new ChildModel { ID = "CHILD_ONE" });

        parent.AddChild(new ChildModel { ID = "CHILD_TWO" });

        _dto = Mapper.Map<ParentModel, ParentDto>(parent);
    }

    [Fact]
    public void Should_preserve_the_parent_child_relationship_on_the_destination()
    {
        _dto.Children[0].Parent.ShouldBeSameAs(_dto);
        _dto.Children[1].Parent.ShouldBeSameAs(_dto);
    }

    public class ParentModel
    {
        public ParentModel()
        {
            Children = new List<ChildModel>();
        }

        public string ID { get; set; }

        public IList<ChildModel> Children { get; private set; }

        public void AddChild(ChildModel child)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }

    public class ChildModel
    {
        public string ID { get; set; }
        public ParentModel Parent { get; set; }
    }

    public class ParentDto
    {
        public string ID { get; set; }
        public IList<ChildDto> Children { get; set; }
    }

    public class ChildDto
    {
        public string ID { get; set; }
        public ParentDto Parent { get; set; }
    }
}


//public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship_using_CustomMapper_StackOverflow : AutoMapperSpecBase
//{
//    private ParentDto _dto;
//    private ParentModel _parent;

//    protected override void Establish_context()
//    {
//        _parent = new ParentModel
//            {
//                ID = 2
//            };

//        List<ChildModel> childModels = new List<ChildModel>
//            {
//                new ChildModel
//                    {
//                        ID = 1,
//                        Parent = _parent
//                    }
//            };

//        Dictionary<int, ParentModel> parents = childModels.ToDictionary(x => x.ID, x => x.Parent);

//        Mapper.CreateMap<int, ParentDto>().ConvertUsing(new ChildIdToParentDtoConverter(parents));
//        Mapper.CreateMap<int, List<ChildDto>>().ConvertUsing(new ParentIdToChildDtoListConverter(childModels));

//        Mapper.CreateMap<ParentModel, ParentDto>()
//            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ID));
//        Mapper.CreateMap<ChildModel, ChildDto>();

//        config.AssertConfigurationIsValid();
//    }

//    protected override void Because_of()
//    {
//        _dto = Mapper.Map<ParentModel, ParentDto>(_parent);
//    }

//    [Fact(Skip = "This test breaks the Test Runner")]
//    public void Should_preserve_the_parent_child_relationship_on_the_destination()
//    {
//        _dto.Children[0].Parent.ID.ShouldBe(_dto.ID);
//    }

//    public class ChildIdToParentDtoConverter : ITypeConverter<int, ParentDto>
//    {
//        private readonly Dictionary<int, ParentModel> _parentModels;

//        public ChildIdToParentDtoConverter(Dictionary<int, ParentModel> parentModels)
//        {
//            _parentModels = parentModels;
//        }

//        public ParentDto Convert(int childId)
//        {
//            ParentModel parentModel = _parentModels[childId];
//            MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
//            return mappingEngine.Map<ParentModel, ParentDto>(parentModel);
//        }
//    }

//    public class ParentIdToChildDtoListConverter : ITypeConverter<int, List<ChildDto>>
//    {
//        private readonly IList<ChildModel> _childModels;

//        public ParentIdToChildDtoListConverter(IList<ChildModel> childModels)
//        {
//            _childModels = childModels;
//        }

//        protected override List<ChildDto> ConvertCore(int childId)
//        {
//            List<ChildModel> childModels = _childModels.Where(x => x.Parent.ID == childId).ToList();
//            MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
//            return mappingEngine.Map<List<ChildModel>, List<ChildDto>>(childModels);
//        }
//    }

//    public class ParentModel
//    {
//        public int ID { get; set; }
//    }

//    public class ChildModel
//    {
//        public int ID { get; set; }
//        public ParentModel Parent { get; set; }
//    }

//    public class ParentDto
//    {
//        public int ID { get; set; }
//        public List<ChildDto> Children { get; set; }
//    }

//    public class ChildDto
//    {
//        public int ID { get; set; }
//        public ParentDto Parent { get; set; }
//    }
//}

public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_one_child_relationship : AutoMapperSpecBase
{
    private FooDto _dto;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Foo, FooDto>();
        cfg.CreateMap<Bar, BarDto>();
    });

    protected override void Because_of()
    {
        var foo = new Foo
            {
                Bar = new Bar
                    {
                        Value = "something"
                    }
            };
        foo.Bar.Foo = foo;
        _dto = Mapper.Map<Foo, FooDto>(foo);
    }

    [Fact]
    public void Should_preserve_the_parent_child_relationship_on_the_destination()
    {
        _dto.Bar.Foo.ShouldBeSameAs(_dto);
    }

    public class Foo
    {
        public Bar Bar { get; set; }
    }

    public class Bar
    {
        public Foo Foo { get; set; }
        public string Value { get; set; }
    }

    public class FooDto
    {
        public BarDto Bar { get; set; }
    }

    public class BarDto
    {
        public FooDto Foo { get; set; }
        public string Value { get; set; }
    }
}

public class When_mapping_to_a_destination_containing_two_dtos_mapped_from_the_same_source : AutoMapperSpecBase
{
    private FooContainerModel _dto;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<FooModel, FooScreenModel>();
        cfg.CreateMap<FooModel, FooInputModel>();
        cfg.CreateMap<FooModel, FooContainerModel>()
            .ForMember(dest => dest.Input, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Screen, opt => opt.MapFrom(src => src));
    });

    protected override void Because_of()
    {
        var model = new FooModel { Id = 3 };
        _dto = Mapper.Map<FooModel, FooContainerModel>(model);
    }

    [Fact]
    public void Should_not_preserve_identity_when_destinations_are_incompatible()
    {
        _dto.ShouldBeOfType<FooContainerModel>();
        _dto.Input.ShouldBeOfType<FooInputModel>();
        _dto.Screen.ShouldBeOfType<FooScreenModel>();
        _dto.Input.Id.ShouldBe(3);
        _dto.Screen.Id.ShouldBe("3");
    }

    public class FooContainerModel
    {
        public FooInputModel Input { get; set; }
        public FooScreenModel Screen { get; set; }
    }

    public class FooScreenModel
    {
        public string Id { get; set; }
    }

    public class FooInputModel
    {
        public long Id { get; set; }
    }

    public class FooModel
    {
        public long Id { get; set; }
    }
}

public class When_mapping_with_a_bidirectional_relationship_that_includes_arrays : AutoMapperSpecBase

{
    private ParentDto _dtoParent;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Parent, ParentDto>();
        cfg.CreateMap<Child, ChildDto>();

    });

    protected override void Because_of()
    {
        var parent1 = new Parent { Name = "Parent 1" };
        var child1 = new Child { Name = "Child 1" };

        parent1.Children.Add(child1);
        child1.Parents.Add(parent1);

        _dtoParent = Mapper.Map<Parent, ParentDto>(parent1);
    }

    [Fact]
    public void Should_map_successfully()
    {
        object.ReferenceEquals(_dtoParent.Children[0].Parents[0], _dtoParent).ShouldBeTrue();
    }

    public class Parent
    {
        public Guid Id { get; private set; }

        public string Name { get; set; }

        public List<Child> Children { get; set; }

        public Parent()
        {
            Id = Guid.NewGuid();
            Children = new List<Child>();
        }

        public bool Equals(Parent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Parent)) return false;
            return Equals((Parent) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class Child
    {
        public Guid Id { get; private set; }

        public string Name { get; set; }

        public List<Parent> Parents { get; set; }

        public Child()
        {
            Id = Guid.NewGuid();
            Parents = new List<Parent>();
        }

        public bool Equals(Child other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Child)) return false;
            return Equals((Child) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ParentDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<ChildDto> Children { get; set; }

        public ParentDto()
        {
            Children = new List<ChildDto>();
        }
    }

    public class ChildDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<ParentDto> Parents { get; set; }

        public ChildDto()
        {
            Parents = new List<ParentDto>();
        }
    }
}