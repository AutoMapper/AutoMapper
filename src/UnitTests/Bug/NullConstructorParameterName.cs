using System.Reflection.Emit;

namespace AutoMapper.UnitTests.Bug;

public class NullConstructorParameterName
{
    [Fact]
    public void ShouldBeSkipped()
    {
        var proxy = CreateDynamicObject();
        var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(ResourcePointDTO), proxy.GetType()));
    }

    public class ResourcePointDTO { }

    object CreateDynamicObject()
    {
        var assemblyName = new AssemblyName("TestClass");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        TypeBuilder typeBuilder = moduleBuilder.DefineType(assemblyName.FullName
            , TypeAttributes.Public |
              TypeAttributes.Class |
              TypeAttributes.AutoClass |
              TypeAttributes.AnsiClass |
              TypeAttributes.BeforeFieldInit |
              TypeAttributes.AutoLayout
            , null);
        typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        var cBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new []{ typeof(int) });
        ILGenerator myConstructorIL = cBuilder.GetILGenerator();
        myConstructorIL.Emit(OpCodes.Ret);
        var type = typeBuilder.CreateType();
        return Activator.CreateInstance(type);
    }
}
