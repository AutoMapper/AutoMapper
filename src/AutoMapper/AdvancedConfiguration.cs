using System;

namespace AutoMapper
{
    public class AdvancedConfiguration
    {
        /// <summary>
        /// Action called against the IConfigurationProvider before it gets sealed
        /// </summary>
        public Action<IConfigurationProvider> BeforeSeal { get; set; }
    }
}