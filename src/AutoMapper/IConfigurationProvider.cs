using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
        void AssertConfigurationIsValid();

        /// <summary>
        /// Create a mapper instance based on this configuration. Mapper instances are lightweight and can be created as needed.
        /// </summary>
        /// <returns>The mapper instance</returns>
        IMapper CreateMapper();

        /// <summary>
        /// Create a mapper instance with the specified service constructor to be used for resolvers and type converters.
        /// </summary>
        /// <param name="serviceCtor">Service factory to create services</param>
        /// <returns>The mapper instance</returns>
        IMapper CreateMapper(Func<Type, object> serviceCtor);

        /// <summary>
        /// Builds the execution plan used to map the source to destination.
        /// Useful to understand what exactly is happening during mapping.
        /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the wiki</a> for details.
        /// </summary>
        /// <param name="sourceType">the runtime type of the source object</param>
        /// <param name="destinationType">the runtime type of the destination object</param>
        /// <returns>the execution plan</returns>
        LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType);

        /// <summary>
        /// Compile all underlying mapping expressions to cached delegates.
        /// Use if you want AutoMapper to compile all mappings up front instead of deferring expression compilation for each first map.
        /// </summary>
        void CompileMappings();
    }
}