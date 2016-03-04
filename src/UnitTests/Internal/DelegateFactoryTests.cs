using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    using Execution;

    public class DelegateFactoryTests
	{
        protected DelegateFactory DelegateFactory => new DelegateFactory();

	    [Fact]
		public void MethodTests()
		{
			MethodInfo method = typeof(String).GetMethod("StartsWith", new[] { typeof(string) });
			LateBoundMethod<string, bool> callback = DelegateFactory.CreateGet<string, bool>(method).Compile();

			string foo = "this is a test";
			bool result = callback(foo, new[] { "this" });

			result.ShouldBeTrue();
		}

		[Fact]
		public void PropertyTests()
		{
			PropertyInfo property = typeof(Source).GetProperty("Value", typeof(int));
			LateBoundPropertyGet<Source, int> callback = DelegateFactory.CreateGet<Source, int>(property).Compile();

			var source = new Source {Value = 5};
			int result = callback(source);

			result.ShouldEqual(5);
		}

		[Fact]
		public void FieldTests()
		{
			FieldInfo field = typeof(Source).GetField("Value2");
			LateBoundFieldGet<Source, int> callback = DelegateFactory.CreateGet<Source, int>(field).Compile();

			var source = new Source {Value2 = 15};
			int result = callback(source);

			result.ShouldEqual(15);
		}

		[Fact]
		public void Should_set_field_when_field_is_a_value_type()
		{
			var sourceType = typeof (Source);
			FieldInfo field = sourceType.GetField("Value2");
			LateBoundFieldSet<Source, int> callback = DelegateFactory.CreateSet<Source, int>(field).Compile();

			var source = new Source();
			callback(source, 5);

			source.Value2.ShouldEqual(5);
		}

		[Fact]
		public void Should_set_field_when_field_is_a_reference_type()
		{
			var sourceType = typeof (Source);
			FieldInfo field = sourceType.GetField("Value3");
			LateBoundFieldSet<Source, string> callback = DelegateFactory.CreateSet<Source, string>(field).Compile();

			var source = new Source();
			callback(source, "hello");

			source.Value3.ShouldEqual("hello");
		}

		[Fact]
		public void Should_set_property_when_property_is_a_value_type()
		{
			var sourceType = typeof (Source);
			PropertyInfo property = sourceType.GetProperty("Value");
			LateBoundPropertySet<Source, int> callback = DelegateFactory.CreateSet<Source, int>(property).Compile();

			var source = new Source();
			callback(source, 5);

			source.Value.ShouldEqual(5);
		}

		[Fact]
		public void Should_set_property_when_property_is_a_value_type_and_type_is_interface()
		{
			var sourceType = typeof (ISource);
			PropertyInfo property = sourceType.GetProperty("Value");
			LateBoundPropertySet<Source, int> callback = DelegateFactory.CreateSet<Source, int>(property).Compile();

			var source = new Source();
			callback(source, 5);

			source.Value.ShouldEqual(5);
		}

		[Fact]
		public void Should_set_property_when_property_is_a_reference_type()
		{
			var sourceType = typeof(Source);
			PropertyInfo property = sourceType.GetProperty("Value4");
			LateBoundPropertySet<Source, string> callback = DelegateFactory.CreateSet<Source, string>(property).Compile();

			var source = new Source();
			callback(source, "hello");

			source.Value4.ShouldEqual("hello");
		}

		internal delegate void DoIt3(ref ValueSource source, string value);

		private void SetValue(object thing, object value)
		{
			var source = ((ValueSource) thing);
			source.Value = (string)value;
		}

        [Fact]
		public void Test_with_create_ctor()
		{
			var sourceType = typeof(Source);

			LateBoundCtor ctor = DelegateFactory.CreateCtor(sourceType);

			var target = ctor();

			target.ShouldBeType<Source>();
		}

		[Fact]
		public void Test_with_value_object_create_ctor()
		{
			var sourceType = typeof(ValueSource);

			LateBoundCtor ctor = DelegateFactory.CreateCtor(sourceType);

			var target = ctor();

			target.ShouldBeType<ValueSource>();
		}

        [Fact]
        public void Create_ctor_should_throw_when_default_constructor_is_missing()
        {
            var type = typeof(NoDefaultConstructor);
            new Action(()=>DelegateFactory.CreateCtor(type)).ShouldThrow<ArgumentException>(ex=>
            {
                ex.Message.ShouldStartWith(type.FullName);
            });
        }

        public object CreateValueSource()
		{
			return new ValueSource();
		}

		public delegate void SetValueDelegate(ref ValueSource source, string value);

		private static void SetValue2(ref object thing, object value)
		{
			var source = ((ValueSource)thing);
			source.Value = (string)value;
			thing = source;
		}

		private void SetValue(ref ValueSource thing, string value)
		{
			thing.Value = value;
		}

		private void DoIt(object source, object value)
		{
			((Source)source).Value2 = (int)value;
		}

		private void DoIt4(object source, object value)
		{
			var valueSource = ((ValueSource)source);
			valueSource.Value = (string)value;
		}

		private void DoIt2(object source, object value)
		{
			int toSet = value == null ? default(int) : (int) value;
			((Source)source).Value = toSet;
		}

		private void DoIt4(ref object source, object value)
		{
			var valueSource = (ValueSource) source;
			valueSource.Value = (string) value;
		}

		private static class Test<T>
		{
			private static T DoIt()
			{
				return default(T);
			}
		}

        public class NoDefaultConstructor
        {
            public NoDefaultConstructor(int x)
            {
            }
        }


		public struct ValueSource
		{
			public string Value { get; set; }
		}

		public interface ISource
		{
			int Value { get; set; }
		}

		public class Source : ISource
		{
			public int Value { get; set; }
			public int Value2;
			public string Value3;
			public string Value4 { get; set; }
		}
	}
}