using System;
using System.Collections.Generic;

namespace AutoMapper.Configuration
{
    public class MapperConfiguration
    {
        public MapperConfiguration(Action<MapperRegistry> initializationExpression)
        {
            if (initializationExpression == null) throw new ArgumentNullException("initializationExpression");

            var registry = new MapperRegistry();

            initializationExpression(registry);
        }

        public IEnumerable<TypeMapConfiguration> TypeMaps
        {
            get { return new TypeMapConfiguration[0]; }
        }
    }
}