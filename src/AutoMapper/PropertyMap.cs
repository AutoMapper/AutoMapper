using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
	public class PropertyMap
	{
		private readonly LinkedList<IValueResolver> _sourceValueResolvers = new LinkedList<IValueResolver>();
		private readonly IList<Type> _valueFormattersToSkip = new List<Type>();
		private readonly IList<IValueFormatter> _valueFormatters = new List<IValueFormatter>();
		private bool _ignored;
		private bool _hasCustomValueResolver = false;

		public PropertyMap(PropertyInfo destinationProperty)
		{
			DestinationProperty = destinationProperty;
		}

		public PropertyMap(PropertyInfo destinationProperty, IEnumerable<IValueResolver> valueResolvers)
		{
			DestinationProperty = destinationProperty;
			ChainResolvers(valueResolvers);
		}

		public PropertyInfo DestinationProperty { get; private set; }

		public IValueResolver[] GetSourceValueResolvers()
		{
			return _sourceValueResolvers.ToArray();
		}

		public void RemoveLastResolver()
		{
			_sourceValueResolvers.RemoveLast();
		}

		public void ChainResolver(IValueResolver IValueResolver)
		{
			_sourceValueResolvers.AddLast(IValueResolver);
		}

		public IValueResolver GetLastResolver()
		{
			return _sourceValueResolvers.Last.Value;
		}

		public void ChainResolvers(IEnumerable<IValueResolver> valueResolvers)
		{
			_sourceValueResolvers.Clear();

			valueResolvers.ForEach(ChainResolver);
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
			ResetSourceMemberChain();
			ChainResolver(valueResolver);
			_hasCustomValueResolver = true;
		}

		public void ChainTypeMemberForResolver(IValueResolver valueResolver)
		{
			_sourceValueResolvers.AddFirst(valueResolver);
		}

		public void Ignore()
		{
			_ignored = true;
		}

		public void ResetSourceMemberChain()
		{
			_sourceValueResolvers.Clear();
		}

		public bool IsMapped()
		{
			return _sourceValueResolvers.Count > 0 || _hasCustomValueResolver || _ignored;
		}


	}
}
