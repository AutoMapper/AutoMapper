namespace AutoMapper
{
    using System;
    using System.Collections.Generic;

    public interface IConfiguration : IProfileConfiguration
    {
        bool AllowNullCollections { get; set; }
        Func<Type, object> ServiceCtor { get; }
        IEnumerable<Profile> Profiles { get; }
    }
}