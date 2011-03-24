using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    public class TypeInfo
    {
        private readonly MemberInfo[] _publicGetters;
        private readonly MemberInfo[] _publicAccessors;
        private readonly MethodInfo[] _publicGetMethods;

        public Type Type { get; private set; }

        public TypeInfo(Type type)
        {
            Type = type;
        	var publicReadableMembers = GetAllPublicReadableMembers();
			_publicGetters = BuildPublicReadAccessors(publicReadableMembers);
			_publicAccessors = BuildPublicAccessors(publicReadableMembers);
            _publicGetMethods = BuildPublicNoArgMethods();
        }

        public IEnumerable<MemberInfo> GetPublicReadAccessors()
        {
            return _publicGetters;
        }

		public IEnumerable<MemberInfo> GetPublicWriteAccessors()
        {
            return _publicAccessors;
        }

        public IEnumerable<MethodInfo> GetPublicNoArgMethods()
        {
            return _publicGetMethods;
        }

        private MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers)
        {
			// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo));  // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

        private MemberInfo[] BuildPublicAccessors(IEnumerable<MemberInfo> allMembers)
        {
        	// Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            var filteredMembers = allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x =>
                    x.Any(y => y.CanWrite && y.CanRead) ? // favor the first property that can both read & write - otherwise pick the first one
						x.Where(y => y.CanWrite && y.CanRead).First() :
                        x.First())
				.Where(pi => pi.CanWrite || pi.PropertyType.IsListOrDictionaryType())
                .OfType<MemberInfo>() // cast back to MemberInfo so we can add back FieldInfo objects
                .Concat(allMembers.Where(x => x is FieldInfo));  // add FieldInfo objects back

            return filteredMembers.ToArray();
        }

    	private IEnumerable<MemberInfo> GetAllPublicReadableMembers()
    	{
			// Collect that target type, its base type, and all implemented/inherited interface types
            var typesToScan = new List<Type>();
            for (var t = Type; t != null; t = t.BaseType)
                typesToScan.Add(t);

    		if (Type.IsInterface)
    			typesToScan.AddRange(Type.GetInterfaces());

    		// Scan all types for public properties and fields
    		return typesToScan
    			.Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
    			.SelectMany(x => x.FindMembers(MemberTypes.Property | MemberTypes.Field,
    			                               BindingFlags.Instance | BindingFlags.Public,
    			                               (m, f) =>
    			                               m is FieldInfo ||
    			                               (m is PropertyInfo && ((PropertyInfo)m).CanRead && !((PropertyInfo)m).GetIndexParameters().Any()), null));
    	}

    	private MethodInfo[] BuildPublicNoArgMethods()
        {
            return Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => (m.ReturnType != null) && (m.GetParameters().Length == 0) && (m.MemberType == MemberTypes.Method))
                .ToArray();
        }
    }
}