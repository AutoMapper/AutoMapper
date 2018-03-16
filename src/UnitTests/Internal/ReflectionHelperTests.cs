

namespace AutoMapper.UnitTests.Internal
{
    using AutoMapper.Internal;
    using System.Collections.Generic;
    using Xunit;

    public class ReflectionHelperTests
    {
        [Fact]
        public void IListOfString_is_assignable_from_ListOfString()
        {
            IList<string> list = new List<string>();
            //Assert
            Assert.True(typeof(List<string>).CanImplicitConvert(typeof(IList<string>)));
        }

        [Fact]
        public void Int_is_assignable_from_short()
        {
            int i = (short)0;
            //Assert
            Assert.True(typeof(short).CanImplicitConvert(typeof(int)));
        }

        [Fact]
        public void Nullable_custom_string_is_assignable_from_custom_string()
        {
            CustString? ss = new CustString("");
            //Assert
            Assert.True(typeof(CustString).CanImplicitConvert(typeof(CustString?)));
        }

        [Fact]
        public void Nullable_int_digit_is_assignable_from_short()
        {
            IntDigit? i = (short)0;
            //Assert
            Assert.True(typeof(short).CanImplicitConvert(typeof(IntDigit?)));
        }

        [Fact]
        public void Nullable_int_is_assignable_from_int()
        {
            int? i = int.MaxValue;
            //Assert
            Assert.True(typeof(int).CanImplicitConvert(typeof(int?)));
        }
        
        [Fact]
        public void Nullable_int_is_assignable_from_nullable_short()
        {
            int? i = new short?();
            //Assert
            Assert.True(typeof(short?).CanImplicitConvert(typeof(int?)));
        }

        [Fact]
        public void Nullable_int_is_assignable_from_short()
        {
            int? i = (short)0;
            //Assert
            Assert.True(typeof(short).CanImplicitConvert(typeof(int?)));
        }

        [Fact]
        public void Short_is_assignable_from_sbyte()
        {
            short s = (sbyte)0;
            //Assert
            Assert.True(typeof(sbyte).CanImplicitConvert(typeof(short)));
        }

        public struct CustString
        {
            public CustString(string i) { val = i; }
            public string val;

            public static implicit operator string(CustString i)
            {
                return i.val;
            }

            public static implicit operator CustString(string i)
            {
                return new CustString(i);
            }
        }

        public struct IntDigit
        {
            public IntDigit(int i) { val = i; }
            public int val;

            public static implicit operator int(IntDigit i)
            {
                return i.val;
            }

            public static implicit operator IntDigit(int i)
            {
                return new IntDigit(i);
            }
        }
    }
}
