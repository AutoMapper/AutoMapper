using System;
using System.Collections.Generic;

namespace AutoMapper
{
    using Validator = Action<ValidationContext>;

    public class AdvancedConfiguration
    {
        private readonly List<Validator> _validators = new List<Validator>();
        private readonly IList<Action<IConfigurationProvider>> _beforeSealActions = new List<Action<IConfigurationProvider>>();
        public IEnumerable<Action<IConfigurationProvider>> BeforeSealActions => _beforeSealActions;

        /// <summary>
        /// Add Action called against the IConfigurationProvider before it gets sealed
        /// </summary>
        public void BeforeSeal(Action<IConfigurationProvider> action) => _beforeSealActions.Add(action);

        /// <summary>
        /// Add an action to be called when validating the configuration.
        /// </summary>
        /// <param name="validator">the validation callback</param>
        public void Validator(Validator validator) => _validators.Add(validator);

        public bool AllowAdditiveTypeMapCreation { get; set; }

        /// <summary>
        /// How many levels deep should AutoMapper try to inline the execution plan for child classes.
        /// See <a href="https://github.com/AutoMapper/AutoMapper/wiki/Understanding-your-mapping">the wiki</a> for details.
        /// </summary>
        public int MaxExecutionPlanDepth { get; set; } = 1;

        internal Validator[] GetValidators() => _validators.ToArray();
    }
}