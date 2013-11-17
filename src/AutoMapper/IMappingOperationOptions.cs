using System;

namespace AutoMapper
{
    using System.Collections.Generic;

    /// <summary>
    /// Options for a single map operation
    /// </summary>
    public interface IMappingOperationOptions
    {
        /// <summary>
        /// Construct services using this callback. Use this for child/nested containers
        /// </summary>
        /// <param name="constructor"></param>
        void ConstructServicesUsing(Func<Type, object> constructor);

        /// <summary>
        /// Create any missing type maps, if found
        /// </summary>
        bool CreateMissingTypeMaps { get; set; }

        /// <summary>
        /// Add context items to be accessed at map time inside an <see cref="IValueResolver"/> or <see cref="ITypeConverter{TSource, TDestination}"/>
        /// </summary>
        IDictionary<string, object> Items { get; }
    }

    public class MappingOperationOptions : IMappingOperationOptions
    {
        public MappingOperationOptions()
        {
            Items = new Dictionary<string, object>();
        }

        public Func<Type, object> ServiceCtor { get; private set; }
        public bool CreateMissingTypeMaps { get; set; }
        public IDictionary<string, object> Items { get; private set; }

        void IMappingOperationOptions.ConstructServicesUsing(Func<Type, object> constructor)
        {
            ServiceCtor = constructor;
        }
    }
}
