//using AutoMapper.Mappers;

//namespace AutoMapper.Internal
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Reflection;
//    using System.Runtime.CompilerServices;

//    public class ProfileConfiguration : IProfileConfiguration
//    {
//        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

//        public ProfileConfiguration()
//        {
//            _memberConfigurations.Add(new MemberConfiguration());
//            ConstructorMappingEnabled = true;
//            IncludeSourceExtensionMethods(typeof(Enumerable).Assembly());
//            ShouldMapProperty = p => p.IsPublic();
//            ShouldMapField = f => f.IsPublic;
//        }

//        private readonly IList<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();
//        public IEnumerable<IMemberConfiguration> MemberConfigurations => _memberConfigurations;

//        private readonly IList<IConditionalObjectMapper> _typeConfigurations = new List<IConditionalObjectMapper>();
//        public IEnumerable<IConditionalObjectMapper> TypeConfigurations => _typeConfigurations;

//        public IMemberConfiguration AddMemberConfiguration()
//        {
//            var condition = new MemberConfiguration();
//            _memberConfigurations.Add(condition);
//            return condition;
//        }

//        public IConditionalObjectMapper AddConditionalObjectMapper()
//        {
//            var condition = new ConditionalObjectMapper(MapperConfiguration.DefaultProfileName);
//            _typeConfigurations.Add(condition);
//            return condition;
//        }

//        public void DisableConstructorMapping()
//        {
//            ConstructorMappingEnabled = false;
//        }

//        public bool ConstructorMappingEnabled { get; private set; }

//        public IMemberConfiguration DefaultMemberConfig { get; }

//        public IEnumerable<MethodInfo> SourceExtensionMethods => _sourceExtensionMethods;

//        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }

//        public Func<FieldInfo, bool> ShouldMapField { get; set; }

//        public void IncludeSourceExtensionMethods(Assembly assembly)
//        {
//            //http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
//            _sourceExtensionMethods.AddRange(assembly.ExportedTypes
//                .Where(type => type.IsSealed() && !type.IsGenericType() && !type.IsNested)
//                .SelectMany(type => type.GetDeclaredMethods().Where(mi => mi.IsStatic))
//                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
//                .Where(method => method.GetParameters().Length == 1));
//        }
//    }
//}