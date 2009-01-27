using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
	public class PropertyMap
	{
		private readonly LinkedList<TypeMember> _sourceMemberChain = new LinkedList<TypeMember>();
		private readonly IList<Type> _valueFormattersToSkip = new List<Type>();
		private readonly IList<IValueFormatter> _valueFormatters = new List<IValueFormatter>();
		private string _nullSubstitute;
		private IValueResolver _valueResolver;
		private bool _hasMembersToResolveForCustomResolver;
		private bool _ignored;

		public PropertyMap(PropertyInfo destinationProperty)
		{
			DestinationProperty = destinationProperty;
		}

		public PropertyMap(PropertyInfo destinationProperty, IEnumerable<TypeMember> typeMembers)
		{
			DestinationProperty = destinationProperty;
			ChainTypeMembers(typeMembers);
		}

		public bool HasMembersToResolveForCustomResolver
		{
			get { return _hasMembersToResolveForCustomResolver; }
		}

		public bool HasCustomValueResolver()
		{
			return _valueResolver != null;
		}

		public PropertyInfo DestinationProperty { get; private set; }

		public bool Ignored
		{
			get { return _ignored; }
		}

		public TypeMember[] GetSourceMemberChain()
		{
			return _sourceMemberChain.ToArray();
		}

		public void RemoveLastModelProperty()
		{
			_sourceMemberChain.RemoveLast();
		}

		public void ChainTypeMember(TypeMember typeMember)
		{
			_sourceMemberChain.AddLast(typeMember);
		}

		public TypeMember GetLastModelMemberInChain()
		{
			return _sourceMemberChain.Last.Value;
		}

		public void ChainTypeMembers(IEnumerable<TypeMember> typeMembers)
		{
			_sourceMemberChain.Clear();
			typeMembers.ForEach(ChainTypeMember);
		}

		public void AddFormatterToSkip<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			_valueFormattersToSkip.Add(typeof(TValueFormatter));
		}

		public bool FormattersToSkipContains(Type valueFormatterType)
		{
			return _valueFormattersToSkip.Contains(valueFormatterType);
		}

		public void AddFormatter(IValueFormatter valueFormatter)
		{
			_valueFormatters.Add(valueFormatter);
		}

		public IValueFormatter[] GetFormatters()
		{
			return _valueFormatters.ToArray();
		}

		public void FormatNullValueAs(string nullSubstitute)
		{
			_nullSubstitute = nullSubstitute;
		}

		public object GetNullSubstitute()
		{
			return _nullSubstitute;
		}

		public void AssignCustomValueResolver(IValueResolver valueResolver)
		{
			_valueResolver = valueResolver;
		}

		public IValueResolver GetCustomValueResolver()
		{
			return _valueResolver;
		}

		public void ChainTypeMembersForResolver(IEnumerable<TypeMember> typeMembers)
		{
			ChainTypeMembers(typeMembers);
			_hasMembersToResolveForCustomResolver = true;
		}

		public bool HasSourceMember()
		{
			return _sourceMemberChain.Count > 0;
		}

		public void Ignore()
		{
			_ignored = true;
		}
	}
}