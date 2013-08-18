using System;

namespace AutoMapper
{
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
    }

    public class MappingOperationOptions : IMappingOperationOptions
    {
        private Func<Type, object> _serviceCtor;

        public Func<Type, object> ServiceCtor
        {
            get { return _serviceCtor; }
        }

        public bool CreateMissingTypeMaps { get; set; }

        public void ConstructServicesUsing(Func<Type, object> constructor)
        {
            _serviceCtor = constructor;
        }
    }
}