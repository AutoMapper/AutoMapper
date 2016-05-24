namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Mappers;

    public class MapperConfigurationExpression : IMapperConfigurationExpression
    {
        private readonly Profile _defaultProfile;
        private readonly IList<Profile> _profiles = new List<Profile>();
        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions = new List<Action<TypeMap, IMappingExpression>>();

        public MapperConfigurationExpression()
        {
            _defaultProfile = new NamedProfile(ProfileName);
            _profiles.Add(_defaultProfile);
        }

        public string ProfileName => "";
        public IEnumerable<Profile> Profiles => _profiles;
        public IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions => _allTypeMapActions;
        public Func<Type, object> ServiceCtor { get; private set; } = ObjectCreator.CreateObject;

        public void CreateProfile(string profileName, Action<Profile> config)
        {
            var profile = new NamedProfile(profileName);

            config(profile);

            AddProfile(profile);
        }

        private class NamedProfile : Profile
        {
            public NamedProfile(string profileName) : base(profileName)
            {
            }
        }

        public void AddProfile(Profile profile)
        {
            profile.Initialize();
            _profiles.Add(profile);
        }

        public void AddProfile<TProfile>() where TProfile : Profile, new() => AddProfile(new TProfile());

        public void AddProfile(Type profileType) => AddProfile((Profile)Activator.CreateInstance(profileType));

        public void ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;

        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get { return _defaultProfile.ShouldMapProperty; }
            set { _defaultProfile.ShouldMapProperty = value; }
        }

        public Func<FieldInfo, bool> ShouldMapField
        {
            get { return _defaultProfile.ShouldMapField; }
            set { _defaultProfile.ShouldMapField = value; }
        }

        public bool CreateMissingTypeMaps
        {
            get { return _defaultProfile.CreateMissingTypeMaps; }
            set { _defaultProfile.CreateMissingTypeMaps = value; }
        }

        public void IncludeSourceExtensionMethods(Type type) => _defaultProfile.IncludeSourceExtensionMethods(type);

        public INamingConvention SourceMemberNamingConvention
        {
            get { return _defaultProfile.SourceMemberNamingConvention; }
            set { _defaultProfile.SourceMemberNamingConvention = value; }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get { return _defaultProfile.DestinationMemberNamingConvention; }
            set { _defaultProfile.DestinationMemberNamingConvention = value; }
        }

        public bool AllowNullDestinationValues
        {
            get { return _defaultProfile.AllowNullDestinationValues; }
            set { _defaultProfile.AllowNullDestinationValues = value; }
        }

        public bool AllowNullCollections
        {
            get { return _defaultProfile.AllowNullCollections; }
            set { _defaultProfile.AllowNullCollections = value; }
        }

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
            => _allTypeMapActions.Add(configuration);

        public Conventions.IMemberConfiguration AddMemberConfiguration()
            => _defaultProfile.AddMemberConfiguration();

        public IConditionalObjectMapper AddConditionalObjectMapper()
            => _defaultProfile.AddConditionalObjectMapper();

        public void DisableConstructorMapping() => _defaultProfile.DisableConstructorMapping();

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            => _defaultProfile.CreateMap<TSource, TDestination>();

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(
            MemberList memberList)
            => _defaultProfile.CreateMap<TSource, TDestination>(memberList);

        public IMappingExpression CreateMap(Type sourceType, Type destinationType)
            => _defaultProfile.CreateMap(sourceType, destinationType, MemberList.Destination);

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
            => _defaultProfile.CreateMap(sourceType, destinationType, memberList);

        public void ClearPrefixes() => _defaultProfile.ClearPrefixes();

        public void RecognizeAlias(string original, string alias)
            => _defaultProfile.RecognizeAlias(original, alias);

        public void ReplaceMemberName(string original, string newValue)
            => _defaultProfile.ReplaceMemberName(original, newValue);

        public void RecognizePrefixes(params string[] prefixes)
            => _defaultProfile.RecognizePrefixes(prefixes);

        public void RecognizePostfixes(params string[] postfixes)
            => _defaultProfile.RecognizePostfixes(postfixes);

        public void RecognizeDestinationPrefixes(params string[] prefixes)
            => _defaultProfile.RecognizeDestinationPrefixes(prefixes);

        public void RecognizeDestinationPostfixes(params string[] postfixes)
            => _defaultProfile.RecognizeDestinationPostfixes(postfixes);

        public void AddGlobalIgnore(string startingwith)
            => _defaultProfile.AddGlobalIgnore(startingwith);

    }
}