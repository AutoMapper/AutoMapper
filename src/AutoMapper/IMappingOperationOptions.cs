using System;

namespace AutoMapper
{
    public interface IMappingOperationOptions
    {
        void ConstructServicesUsing(Func<Type, object> constructor);
    }

    public class MappingOperationOptions : IMappingOperationOptions
    {
        private Func<Type, object> _serviceCtor;

        public Func<Type, object> ServiceCtor
        {
            get { return _serviceCtor; }
        }

        public void ConstructServicesUsing(Func<Type, object> constructor)
        {
            _serviceCtor = constructor;
        }
    }
}