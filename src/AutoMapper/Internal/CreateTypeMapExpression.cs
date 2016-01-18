using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.Internal
{
    public class CreateTypeMapExpression : IMappingExpression
    {
        private readonly List<Action<IMappingExpression>> _actions = new List<Action<IMappingExpression>>();

        public TypePair TypePair { get; private set; }
        public MemberList MemberList { get; private set; }
        public string ProfileName { get; private set; }

        public TypeMap TypeMap
        {
            get
            {
                //nothing useful to do here
                throw new NotImplementedException();
            }
        }

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
            foreach(var action in _actions)
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

        public IMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter()
        {
            _actions.Add(me => me.IgnoreAllPropertiesWithAnInaccessibleSetter());
            return this;
        }

        public IMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
        {
            _actions.Add(me => me.IgnoreAllSourcePropertiesWithAnInaccessibleSetter());
            return this;
        }

        public IMappingExpression IncludeBase(Type sourceBase, Type destinationBase)
        {
            _actions.Add(me => me.IncludeBase(sourceBase, destinationBase));
            return this;
        }

        public void ProjectUsing(Expression<Func<object, object>> projectionExpression)
        {
            _actions.Add(me => me.ProjectUsing(projectionExpression));
        }

        public IMappingExpression BeforeMap(Action<object, object> beforeFunction)
        {
            _actions.Add(me => me.BeforeMap(beforeFunction));
            return this;
        }

        public IMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<object, object>
        {
            _actions.Add(me => me.BeforeMap<TMappingAction>());
            return this;
        }

        public IMappingExpression AfterMap(Action<object, object> afterFunction)
        {
            _actions.Add(me => me.AfterMap(afterFunction));
            return this;
        }

        public IMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<object, object>
        {
            _actions.Add(me => me.AfterMap<TMappingAction>());
            return this;
        }

        public IMappingExpression ConstructUsing(Func<object, object> ctor)
        {
            _actions.Add(me => me.ConstructUsing(ctor));
            return this;
        }

        public IMappingExpression ConstructUsing(Func<ResolutionContext, object> ctor)
        {
            _actions.Add(me => me.ConstructUsing(ctor));
            return this;
        }

        public IMappingExpression ConstructProjectionUsing(LambdaExpression ctor)
        {
            _actions.Add(me => me.ConstructProjectionUsing(ctor));
            return this;
        }

        public IMappingExpression MaxDepth(int depth)
        {
            _actions.Add(me => me.MaxDepth(depth));
            return this;
        }

        public IMappingExpression ConstructUsingServiceLocator()
        {
            _actions.Add(me => me.ConstructUsingServiceLocator());
            return this;
        }

        public IMappingExpression Substitute(Func<object, object> substituteFunc)
        {
            _actions.Add(me => me.Substitute(substituteFunc));
            return this;
        }

        public IMappingExpression ReverseMap()
        {
            _actions.Add(me => me.ReverseMap());
            // we can register that ReverseMap was called, but there is nothing we can return
            return null;
        }

        public IMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions)
        {
            _actions.Add(me => me.ForCtorParam(ctorParamName, paramOptions));
            return this;
        }
    }
}