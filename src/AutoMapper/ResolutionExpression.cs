using System;
using System.Linq.Expressions;

namespace AutoMapper
{
	internal class ResolutionExpression<TResolutionModel> : IResolutionExpression<TResolutionModel>
	{
		private readonly PropertyMap _propertyMap;

		public ResolutionExpression(PropertyMap propertyMap)
		{
			_propertyMap = propertyMap;
		}

		public void FromMember(Func<TResolutionModel, object> sourceMember)
		{
			_propertyMap.ChainTypeMemberForResolver(new NewMethod<TResolutionModel>(sourceMember));
		}
	}
}