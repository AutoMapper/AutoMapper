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

            Func<object> ctor = DelegateFactory.CreateCtor(sourceType);

            var target = ctor();

            target.ShouldBeType<Source>();
        }

        [Fact]
        public void Test_with_value_object_create_ctor()
        {
            var sourceType = typeof(ValueSource);

            Func<object> ctor = DelegateFactory.CreateCtor(sourceType);

            var target = ctor();

            target.ShouldBeType<ValueSource>();
        }

        [Fact]
        public void Create_ctor_should_throw_when_default_constructor_is_missing()
        {
            var type = typeof(NoDefaultConstructor);
            new Action(()=>DelegateFactory.CreateCtor(type)()).ShouldThrow<ArgumentException>(ex=>
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