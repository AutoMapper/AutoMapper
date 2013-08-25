using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AutoMapper
{
    public class DelegateFactory : IDelegateFactory
    {
        public LateBoundMethod CreateGet(MethodInfo method)
        {
            return null;
        }

        public LateBoundPropertyGet CreateGet(PropertyInfo property)
        {
            return null;
        }

        public LateBoundFieldGet CreateGet(FieldInfo field)
        {
            return null;
        }

        public LateBoundFieldSet CreateSet(FieldInfo field)
        {
            return null;
        }

        public LateBoundPropertySet CreateSet(PropertyInfo property)
        {
            return null;
        }

        public LateBoundCtor CreateCtor(Type type)
        {
            return null;
        }

        public LateBoundParamsCtor CreateCtor(ConstructorInfo constructorInfo, IEnumerable<ConstructorParameterMap> ctorParams)
        {
            return null;
        }
    }
}
