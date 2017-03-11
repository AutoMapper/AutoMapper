using System;
using System.Collections.Generic;

namespace AutoMapper.Configuration
{
    public interface IConfiguration : IProfileConfiguration
    {
        Func<Type, object> ServiceCtor { get; }
        IEnumerable<IProfileConfiguration> Profiles { get; }
    }
}