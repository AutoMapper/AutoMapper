using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Should;
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

		internal delegate void DoIt3(ref ValueSource source, string value);

		private void SetValue(object thing, object value)
		{
			var source = ((ValueSource) thing);
			source.Value = (string)value;
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

		[Test, Explicit]
		public void Test_with_DynamicMethod2()
		{
			var sourceType = typeof(DelegateFactoryTests);
			MethodInfo property = sourceType.GetMethod("SetValue2", BindingFlags.Static | BindingFlags.NonPublic);

			var d = (LateBoundValueTypePropertySet)Delegate.CreateDelegate(typeof(LateBoundValueTypePropertySet), property);

			object othersource = new ValueSource();
			DoIt4(othersource, "Asdf");

			var source = new ValueSource();

			var value = (object) source;

			d(ref value, "hello");

			source.Value.ShouldEqual("hello");
		}

		[Test, Explicit]
		public void Test_with_CreateDelegate()
		{
			var sourceType = typeof(ValueSource);
			PropertyInfo property = sourceType.GetProperty("Value");

			LateBoundValueTypePropertySet callback = DelegateFactory.CreateValueTypeSet(property);

			var source = new ValueSource();

			var target = ((object)source);

			callback(ref target, "hello");

			source.Value.ShouldEqual("hello");
		}

		[Test]
		public void Test_with_create_ctor()
		{
			var sourceType = typeof(Source);

			LateBoundCtor ctor = DelegateFactory.CreateCtor(sourceType);

			var target = ctor();

			target.ShouldBeType<Source>();
		}

		[Test]
		public void Test_with_value_object_create_ctor()
		{
			var sourceType = typeof(ValueSource);

			LateBoundCtor ctor = DelegateFactory.CreateCtor(sourceType);

			var target = ctor();

			target.ShouldBeType<ValueSource>();
		}

		public object CreateValueSource()
		{
			return new ValueSource();
		}

#if !SILVERLIGHT
		[Test, Explicit]
		public void Test_with_DynamicMethod()
		{
			var sourceType = typeof(ValueSource);
			PropertyInfo property = sourceType.GetProperty("Value");

			var setter = property.GetSetMethod(true);
			var method = new DynamicMethod("Set" + property.Name, null, new[] { typeof(object).MakeByRefType(), typeof(object) }, false);
			var gen = method.GetILGenerator();

			method.InitLocals = true;
			gen.Emit(OpCodes.Ldarg_0); // Load input to stack
			gen.Emit(OpCodes.Ldind_Ref);
			gen.Emit(OpCodes.Unbox_Any, sourceType); // Unbox the source to its correct type
			gen.Emit(OpCodes.Stloc_0); // Store the unboxed input on the stack
			gen.Emit(OpCodes.Ldloca_S, 0);
			gen.Emit(OpCodes.Ldarg_1); // Load value to stack
			gen.Emit(OpCodes.Castclass, property.PropertyType); // Unbox the value to its proper value type
			gen.Emit(OpCodes.Call, setter); // Call the setter method
			gen.Emit(OpCodes.Ret);

			var result = (LateBoundValueTypePropertySet)method.CreateDelegate(typeof(LateBoundValueTypePropertySet));

			var source = new ValueSource();

			var value = (object) source;

			result(ref value, "hello");

			source.Value.ShouldEqual("hello");
		}
#endif

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