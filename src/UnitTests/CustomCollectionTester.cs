namespace AutoMapper.UnitTests;
public class CustomCollectionTester {
    [Fact]
    public void Should_be_able_to_handle_custom_dictionary_with_custom_methods() {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<BaseClassWithDictionary, DerivedClassWithDictionary>());
        var mapper = config.CreateMapper();
    }
    
    public class BaseClassWithDictionary {
        public DataDictionary Data { get; set; }
    }

    public class DerivedClassWithDictionary : BaseClassWithDictionary { }

    public class DataDictionary : Dictionary<string, object> {
        public string GetString(string name, string @default) {
            return null;
        }
    }
}