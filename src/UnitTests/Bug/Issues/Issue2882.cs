using System;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace AutoMapper.UnitTests.Bug.Issues
{
#if NET461
    public class Issue2882
    {
        [Fact]
        public void TestMethod1()
        {
            var proxy = CreateDynamicObject();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.CreateMap(typeof(ResourcePointDTO), proxy.GetType());
            });
            var mapper = config.CreateMapper();
        }

        public class ResourcePointDTO
        { }

        public interface A { }

        object CreateDynamicObject()
        {
            var assemblyName = new AssemblyName("TestClass");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(assemblyName.FullName
                , TypeAttributes.Public |
                  TypeAttributes.Class |
                  TypeAttributes.AutoClass |
                  TypeAttributes.AnsiClass |
                  TypeAttributes.BeforeFieldInit |
                  TypeAttributes.AutoLayout
                , null);
            var fieldBuilder = typeBuilder.DefineField("__interceptors", typeof(A), FieldAttributes.Private);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            var cBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new []{ typeof(A) });
            ILGenerator myConstructorIL = cBuilder.GetILGenerator();
            myConstructorIL.Emit(OpCodes.Ldarg_0);
            myConstructorIL.Emit(OpCodes.Ldarg_1);
            myConstructorIL.Emit(OpCodes.Stfld, fieldBuilder);
            myConstructorIL.Emit(OpCodes.Ret);
            var type = typeBuilder.CreateType();
            return Activator.CreateInstance(type);
        }
    }
#endif
}
