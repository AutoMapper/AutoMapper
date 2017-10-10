<img src="https://s3.amazonaws.com/automapper/logo.png" alt="AutoMapper">

[![Build status](https://ci.appveyor.com/api/projects/status/q261l3sbokafmx1o/branch/develop?svg=true)](https://ci.appveyor.com/project/jbogard/automapper/branch/develop)
[![NuGet](http://img.shields.io/nuget/v/AutoMapper.svg)](https://www.nuget.org/packages/AutoMapper/)
[![MyGet (dev)](https://img.shields.io/myget/automapperdev/v/AutoMapper.svg)](http://myget.org/gallery/automapperdev)

### What is AutoMapper?

AutoMapper is a simple little library built to solve a deceptively complex problem - getting rid of code that mapped one object to another. This type of code is rather dreary and boring to write, so why not invent a tool to do it for us?

This is the main repository for AutoMapper, but there's more:

* [EF6 Extensions](https://github.com/AutoMapper/AutoMapper.EF6)
* [IDataReader/Record Extensions](https://github.com/AutoMapper/AutoMapper.Data)
* [Collection Extensions](https://github.com/AutoMapper/AutoMapper.Collection)
* [Microsoft DI Extensions](https://github.com/AutoMapper/AutoMapper.Extensions.Microsoft.DependencyInjection)

### How do I get started?

First, configure AutoMapper to know what types you want to map, in the startup of your application:

```csharp
Mapper.Initialize(cfg => {
    cfg.CreateMap<Foo, FooDto>();
    cfg.CreateMap<Bar, BarDto>();
});
```
Then in your application code, execute the mappings:

```csharp
var fooDto = Mapper.Map<FooDto>(foo);
var barDto = Mapper.Map<BarDto>(bar);
```

Check out the [getting started guide](http://automapper.readthedocs.io/en/latest/Getting-started.html). When you're done there, the [wiki](http://automapper.readthedocs.io/en/latest/) goes in to the nitty-gritty details. If you have questions, you can post them to [Stack Overflow](http://stackoverflow.com/questions/tagged/automapper) or in our [Gitter](https://gitter.im/AutoMapper/AutoMapper).

### Where can I get it?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [AutoMapper](https://www.nuget.org/packages/AutoMapper/) from the package manager console:

```
PM> Install-Package AutoMapper
```

### Do you have an issue?

First check if it's already fixed by trying the [MyGet build](http://automapper.readthedocs.io/en/latest/The-MyGet-build.html).

You might want to know exactly what [your mapping does](http://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html) at runtime.

If you're still running into problems, file an issue above.

### License, etc.

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

AutoMapper is Copyright &copy; 2009 [Jimmy Bogard](https://jimmybogard.com) and other contributors under the [MIT license](LICENSE.txt).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
