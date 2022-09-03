namespace AutoMapper.UnitTests;
public class TypeExtensionsTests
{
    public class Foo
    {
        public Foo()
        {
            Value2 = "adsf";
            Value4 = "Fasdfadsf";
        }

        public string Value1 { get; set; }
        public string Value2 { get; private set; }
        protected string Value3 { get; set; }
        private string Value4 { get; set; }
        public string Value5 => "ASDf";
        public string Value6 {  set { Value4 = value; } }

        [Fact]
        public void Should_recognize_public_members()
        {
//                typeof(Foo).GetProperties().Length.ShouldBe(4);
        }
    } 
}