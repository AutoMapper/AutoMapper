using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.Modules;

namespace TestModuleAssembly
{
    public class SourceMapperModule : AutoMapperModule
    {
        protected override void OnLoad()
        {
            CreateMap<TAMSource, TAMDestination>();
        }
    }
}
