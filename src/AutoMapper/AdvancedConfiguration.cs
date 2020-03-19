using AutoMapper.QueryableExtensions;
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

        /// <summary>
        /// Allow the same map to exist in different profiles.
        /// The default is to throw an exception, true means the maps are merged.
        /// </summary>
        public bool AllowAdditiveTypeMapCreation { get; set; }

        /// <summary>
        /// How many levels deep should AutoMapper try to inline the execution plan for child classes.
        /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the docs</a> for details.
        /// </summary>
        public int MaxExecutionPlanDepth { get; set; } = 1;

        internal Validator[] GetValidators() => _validators.ToArray();

        public List<IExpressionResultConverter> QueryableResultConverters { get; } = ExpressionBuilder.DefaultResultConverters();

        public List<IExpressionBinder> QueryableBinders { get; } = ExpressionBuilder.DefaultBinders();

        /// <summary>
        /// How many levels deep should recursive queries be expanded.
        /// Must be zero for EF6. Can be greater than zero for EF Core.
        /// </summary>
        public int RecursiveQueriesMaxDepth { get; set; }
    }
}
