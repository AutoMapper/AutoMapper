using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class DelegateFactoryTests
	{
		[Test]
		public void MethodTests()
		{
			MethodInfo method = typeof(String).GetMethod("StartsWith", new[] { typeof(string) });
			LateBoundMethod callback = DelegateFactory.CreateGet(method);

			string foo = "this is a test";
			bool result = (bool)callback(foo, new[] { "this" });

			result.ShouldBeTrue();
		}

		[Test]
		public void PropertyTests()
		{
			PropertyInfo property = typeof(Source).GetProperty("Value", typeof(int));
			LateBoundPropertyGet callback = DelegateFactory.CreateGet(property);

			var source = new Source {Value = 5};
			int result = (int)callback(source);

			result.ShouldEqual(5);
		}

		[Test]
		public void FieldTests()
		{
			FieldInfo field = typeof(Source).GetField("Value2");
			LateBoundFieldGet callback = DelegateFactory.CreateGet(field);

			var source = new Source {Value2 = 15};
			int result = (int)callback(source);

			result.ShouldEqual(15);
		}

		[Test]
		public void Should_set_field_when_field_is_a_value_type()
		{
			var sourceType = typeof (Source);
			FieldInfo field = sourceType.GetField("Value2");
			LateBoundFieldSet callback = DelegateFactory.CreateSet(field);

			var source = new Source();
			callback(source, 5);

			source.Value2.ShouldEqual(5);
		}

		[Test]
		public void Should_set_field_when_field_is_a_reference_type()
		{
			var sourceType = typeof (Source);
			FieldInfo field = sourceType.GetField("Value3");
			LateBoundFieldSet callback = DelegateFactory.CreateSet(field);

			var source = new Source();
			callback(source, "hello");

			source.Value3.ShouldEqual("hello");
		}

		[Test]
		public void Should_set_property_when_property_is_a_value_type()
		{
			var sourceType = typeof (Source);
			PropertyInfo property = sourceType.GetProperty("Value");
			LateBoundPropertySet callback = DelegateFactory.CreateSet(property);

			var source = new Source();
			callback(source, 5);

			source.Value.ShouldEqual(5);
		}

		[Test]
		public void Should_set_property_when_property_is_a_value_type_and_type_is_interface()
		{
			var sourceType = typeof (ISource);
			PropertyInfo property = sourceType.GetProperty("Value");
			LateBoundPropertySet callback = DelegateFactory.CreateSet(property);

			var source = new Source();
			callback(source, 5);

			source.Value.ShouldEqual(5);
		}

		[Test]
		public void Should_set_property_when_property_is_a_reference_type()
		{
			var sourceType = typeof(Source);
			PropertyInfo property = sourceType.GetProperty("Value4");
			LateBoundPropertySet callback = DelegateFactory.CreateSet(property);

			var source = new Source();
			callback(source, "hello");

			source.Value4.ShouldEqual("hello");
		}

		[Test, Explicit]
		public void WhatIWantToDo()
		{
			var sourceType = typeof(Source);
			var property = sourceType.GetProperty("Value");
			var setter = property.GetSetMethod();
			var method = new DynamicMethod("GetValue", null, new[] { typeof(object), typeof(object) }, sourceType);
			var gen = method.GetILGenerator();

			//gen.Emit(OpCodes.Ldarg_0); // Load input to stack
			//gen.Emit(OpCodes.Ldarg_1); // Load value to stack
			//gen.Emit(OpCodes.Stfld, field); // Set the value to the input field
			//gen.Emit(OpCodes.Ret);
			gen.Emit(OpCodes.Ldarg_0); // Load input to stack
			gen.Emit(OpCodes.Castclass, sourceType); // Cast to source type
			gen.Emit(OpCodes.Ldarg_1); // Load value to stack
			gen.Emit(OpCodes.Unbox_Any, property.PropertyType); // Unbox the value to its proper value type
			gen.Emit(OpCodes.Callvirt, property.GetSetMethod()); // Set the value to the input field
			gen.Emit(OpCodes.Ret);

			var result = (LateBoundPropertySet)method.CreateDelegate(typeof(LateBoundPropertySet));

			var source = new Source();
			DateTime start = DateTime.Now;

			for (int i = 0; i < 1000000; i++)
			{
				source.Value = 5;
			}
			var span = DateTime.Now - start;

			Console.WriteLine("Raw:" + span.Ticks);

			start = DateTime.Now;
			for (int i = 0; i < 1000000; i++)
			{
				setter.Invoke(source, new object[] { 5 });
			}
			span = DateTime.Now - start;
			Console.WriteLine("MethodInfo:" + span.Ticks);

			start = DateTime.Now;
			for (int i = 0; i < 1000000; i++)
			{
				result(source, 5);
			}
			span = DateTime.Now - start;
			Console.WriteLine("LCG:" + span.Ticks);

		}

		private void DoIt(object source, object value)
		{
			((Source)source).Value2 = (int)value;
		}

		private void DoIt2(object source, object value)
		{
			int toSet = value == null ? default(int) : (int) value;
			((Source)source).Value = toSet;
		}

		private static class Test<T>
		{
			private static T DoIt()
			{
				return default(T);
			}
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