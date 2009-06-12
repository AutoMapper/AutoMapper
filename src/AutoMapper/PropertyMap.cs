using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class PropertyMap
	{
		private readonly LinkedList<IValueResolver> _sourceValueResolvers = new LinkedList<IValueResolver>();
		private readonly IList<Type> _valueFormattersToSkip = new List<Type>();
		private readonly IList<IValueFormatter> _valueFormatters = new List<IValueFormatter>();
		private bool _ignored;
		private int _mappingOrder;
		private bool _hasCustomValueResolver;
		private IValueResolver _customResolver;
		private IValueResolver _customMemberResolver;
		private object _nullSubstitute;
		private bool _sealed;
		private IValueResolver[] _cachedResolvers;

		public PropertyMap(IMemberAccessor destinationProperty)
		{
			DestinationProperty = destinationProperty;
		}

		public IMemberAccessor DestinationProperty { get; private set; }

	    public bool UseDestinationValue { get; set; }

	    public IEnumerable<IValueResolver> GetSourceValueResolvers()
		{
			yield return new DefaultResolver();

			yield return _customMemberResolver;

			yield return _customResolver;

			foreach (var resolver in _sourceValueResolvers)
			{
				yield return resolver;
			}

			yield return new NullReplacementMethod(_nullSubstitute);
		}

		public void RemoveLastResolver()
		{
			_sourceValueResolvers.RemoveLast();
		}

		public ResolutionResult ResolveValue(object input)
		{
			Seal();

			var result = new ResolutionResult(input);

			foreach (var resolver in _cachedResolvers)
			{
				result = resolver.Resolve(result);
			}

			return result;
		}

		private void Seal()
		{
			if (_sealed)
			{
				return;
			}

			_sealed = true;
			_cachedResolvers = GetSourceValueResolvers().Where(r => r != null).ToArray();
		}

		public void ChainResolver(IValueResolver IValueResolver)
		{
			_sourceValueResolvers.AddLast(IValueResolver);
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

		public void AssignCustomValueResolver(IValueResolver valueResolver)
		{
			_customResolver = valueResolver;
			ResetSourceMemberChain();
			_hasCustomValueResolver = true;
		}

		public void ChainTypeMemberForResolver(IValueResolver valueResolver)
		{
			ResetSourceMemberChain();
			_customMemberResolver = valueResolver;
		}

		public void ChainConstructorForResolver(IValueResolver valueResolver)
		{
			_customResolver = valueResolver;
		}

		public void Ignore()
		{
			_ignored = true;
		}

		public bool IsIgnored()
		{
			return _ignored;
		}

		public void SetMappingOrder(int mappingOrder)
		{
			_mappingOrder = mappingOrder;
		}

		public int GetMappingOrder()
		{
			return _mappingOrder;
		}

		public bool IsMapped()
		{
			return _sourceValueResolvers.Count > 0 || _hasCustomValueResolver || _ignored;
		}

		public bool CanResolveValue()
		{
			return (_sourceValueResolvers.Count > 0 || _hasCustomValueResolver) && !_ignored;
		}

		public void RemoveLastFormatter()
		{
			_valueFormatters.RemoveAt(_valueFormatters.Count - 1);
		}

		public void SetNullSubstitute(object nullSubstitute)
		{
			_nullSubstitute = nullSubstitute;
		}

		private void ResetSourceMemberChain()
		{
			_sourceValueResolvers.Clear();
		}

		public bool Equals(PropertyMap other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.DestinationProperty, DestinationProperty);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(PropertyMap)) return false;
			return Equals((PropertyMap)obj);
		}

		public override int GetHashCode()
		{
			return DestinationProperty.GetHashCode();
		}
	}
}