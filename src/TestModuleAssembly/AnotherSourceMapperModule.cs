using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.Modules;

namespace TestModuleAssembly
{
    public class AnotherSourceMapperModule : AutoMapperModule
    {
        protected override void OnLoad()
        {
            CreateMap<TAMAnotherSource, TAMAnotherDestination>()
                .ForMember(d => d.SomeOtherProperty, opt => opt.MapFrom(s => s.SomeProperty))
                .ForMember(d => d.SomeProperty,      opt => opt.MapFrom(s => s.SomeOtherProperty));
        }
    }   
}
