namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class MemberConfiguration : IMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public IList<IChildMemberConfiguration> MemberMappers { get; } = new Collection<IChildMemberConfiguration>();
        
        public IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new()
        {
            GetOrAdd(_ => (IList)_.MemberMappers, setupAction);
            return this;
        }

        public IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new()
        {
            GetOrAdd(_ => (IList)_.NameMapper.NamedMappers, setupAction);
            return this;
        }

        private TMemberMapper GetOrAdd<TMemberMapper>(Func<IMemberConfiguration, IList> getList, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : new()
        {
            var child = getList(this).OfType<TMemberMapper>().FirstOrDefault();
            if (child == null)
            {
                child = new TMemberMapper();
                getList(this).Add(child);
            }
            setupAction?.Invoke(child);
            return child;
        }

        public MemberConfiguration()
        {
            NameMapper = new ParentSourceToDestinationNameMapper();
            MemberMappers.Add(new DefaultMember { NameMapper = NameMapper });
        }

        public bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers)
        {
            var foundMap = false;
            foreach (var memberMapper in MemberMappers)
            {
                foundMap = memberMapper.MapDestinationPropertyToSource(options, sourceType, destType, nameToSearch, resolvers, this);
                if (foundMap)
                    break;
            }
            return foundMap;
        }
    }
}