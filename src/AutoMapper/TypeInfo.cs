using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoMapper
{
    public class TypeInfo
    {
        private readonly MemberInfo[] _publicGetters;
        private readonly MemberInfo[] _publicAccessors;
        private readonly MethodInfo[] _publicGetMethods;
        private readonly ConstructorInfo[] _constructors;

        public Type Type { get; private set; }

        public TypeInfo(Type type)
        {
            Type = type;
        	var publicReadableMembers = GetAllPublicReadableMembers();
            var publicWritableMembers = GetAllPublicWritableMembers();
			_publicGetters = BuildPublicReadAccessors(publicReadableMembers);
            _publicAccessors = BuildPublicAccessors(publicWritableMembers);
            _publicGetMethods = BuildPublicNoArgMethods();
            _constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public IEnumerable<ConstructorInfo> GetConstructors()
        {
            return _constructors;
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

		public IEnumerable<MethodInfo> GetPublicNoArgExtensionMethods(Assembly[] sourceExtensionMethodSearch)
		{
			if (sourceExtensionMethodSearch == null)
			{
				return new MethodInfo[] { };
			}

			//http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
			return sourceExtensionMethodSearch
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
				.SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				.Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
				.Where(method => method.GetParameters()[0].ParameterType == this.Type);
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
            return GetAllPublicMembers(PropertyReadable, BindingFlags.Instance | BindingFlags.Public);
    	}

        private IEnumerable<MemberInfo> GetAllPublicWritableMembers()
        {
            return GetAllPublicMembers(PropertyWritable, BindingFlags.Instance | BindingFlags.Public);
        }

        private bool PropertyReadable(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead;
        }

        private bool PropertyWritable(PropertyInfo propertyInfo)
        {
            bool propertyIsEnumerable = (typeof(string) != propertyInfo.PropertyType)
                                         && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);

            return propertyInfo.CanWrite || propertyIsEnumerable;
        }

        private IEnumerable<MemberInfo> GetAllPublicMembers(Func<PropertyInfo, bool> propertyAvailableFor, BindingFlags bindingAttr)
        {
            var typesToScan = new List<Type>();
            for (var t = Type; t != null; t = t.BaseType)
                typesToScan.Add(t);

            if (Type.IsInterface)
                typesToScan.AddRange(Type.GetInterfaces());

            // Scan all types for public properties and fields
            return typesToScan
                .Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
                .SelectMany(x => x.GetMembers(bindingAttr)
                    .Where(m => m is FieldInfo || (m is PropertyInfo && propertyAvailableFor((PropertyInfo)m) && !((PropertyInfo)m).GetIndexParameters().Any())));
        }

        private MethodInfo[] BuildPublicNoArgMethods()
        {
            return Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => (m.ReturnType != typeof(void)) && (m.GetParameters().Length == 0))
                .ToArray();
        }
    }
}