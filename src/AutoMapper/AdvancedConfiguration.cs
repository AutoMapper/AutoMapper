using System;
using System.Collections.Generic;

namespace AutoMapper
{
    public class AdvancedConfiguration
    {
        private readonly IList<Action<IConfigurationProvider>> _beforeSealActions = new List<Action<IConfigurationProvider>>();
        public IEnumerable<Action<IConfigurationProvider>> BeforeSealActions => _beforeSealActions;

        /// <summary>
        /// Add Action called against the IConfigurationProvider before it gets sealed
        /// </summary>
        public void BeforeSeal(Action<IConfigurationProvider> action)
        {
            _beforeSealActions.Add(action);
        }
    }
}