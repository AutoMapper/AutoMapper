A bit of example, howto

```c#
var assemblyName = new AssemblyName { Name = "AssemblyName" };
var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

var module = assembly.DefineDynamicModule("Module", assemblyFile, true);
var mappingsType = module.DefineType("MainMappingsType", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

MapperConfigurationExpression config = // init;
config.MustBeGeneratedCompatible = true;
Pro.MethodBuilderFactory = r =>
{
    // Anything can be there. You can concat two FullName's
    // if you want to reflect get it, or create a special
    // method that finds a mapper for given <T1, T2>
    string name = ConvertMethodToName(r.RequestedTypes);
    var method = mappingType.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Static);
    return method;
};

var mapperConfiguration = new MapperConfiguration(config);
mapperConfiguration.AssertConfigurationIsValid();
var mapper = mapperConfiguration.CreateMapper();
mapper.CompileMappings();

assembly.Save(assemblyFile);
```

Note, that since it's not compiling in runtime, it dosen't have permissions to access to any of yours private or internal fields or methods.
You can use internal fields if you specify the [InternalsVisibleToAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.internalsvisibletoattribute?view=net-5.0) to your assembly.
