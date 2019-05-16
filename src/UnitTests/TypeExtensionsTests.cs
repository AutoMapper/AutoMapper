namespace AutoMapper.UnitTests
{
    using Shouldly;
    using Xunit;

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

        public class GetDeclaredMethod_order
        {
            public class MethodOrderTest
            {
                public void M1(object arg1) { }
                public void M1() { }
                public void M1(object arg1, object arg2) { }
            }

            [Fact]
            public void Should_return_first_ordered_by_parameters_count()
            {
                var result = typeof(MethodOrderTest).GetDeclaredMethod(nameof(MethodOrderTest.M1));
                result.GetParameters().ShouldBeEmpty();
            }
        }
    }
}