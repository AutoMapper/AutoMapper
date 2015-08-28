namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using Impl;

    public class CreateTypeMapExpression : IMappingExpression
    {
        private readonly List<Action<IMappingExpression>> _actions = new List<Action<IMappingExpression>>();

        public TypePair TypePair { get; private set; }
        public MemberList MemberList { get; private set; }
        public string ProfileName { get; private set; }
        

        public CreateTypeMapExpression(TypePair typePair, MemberList memberList, string profileName)
        {
            TypePair = typePair;
            MemberList = memberList;
            ProfileName = profileName;
        }

        public void ConvertUsing<TTypeConverter>()
        {
            _actions.Add(me => me.ConvertUsing<TTypeConverter>());
        }

        public void ConvertUsing(Type typeConverterType)
        {
            _actions.Add(me => me.ConvertUsing(typeConverterType));
        }

        public void As(Type typeOverride)
        {
            _actions.Add((me => me.As(typeOverride)));
        }

        public IMappingExpression WithProfile(string profileName)
        {
            _actions.Add(me => me.WithProfile(profileName));
            
            return this;
        }

        public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions)
        {
            _actions.Add(me => me.ForMember(name, memberOptions));

            return this;
        }

        public IMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
        {
            _actions.Add(me => me.ForSourceMember(sourceMemberName, memberOptions));

            return this;
        }

        public void Accept(IMappingExpression mappingExpression)
        {
            foreach (var action in _actions)
            {
                action(mappingExpression);
            }
        }

        public IMappingExpression Include(Type derivedSourceType, Type derivedDestinationType)
        {
            _actions.Add(me => me.Include(derivedSourceType, derivedDestinationType));

            return this;
        }

        public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions)
        {
            _actions.Add(me => me.ForAllMembers(memberOptions));
        }
    }
}