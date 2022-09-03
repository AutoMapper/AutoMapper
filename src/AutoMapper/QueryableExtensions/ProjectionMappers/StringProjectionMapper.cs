using AutoMapper.Execution;
using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;
namespace AutoMapper.QueryableExtensions.Impl;

[EditorBrowsable(EditorBrowsableState.Never)]
public class StringProjectionMapper : IProjectionMapper
{
    public bool IsMatch(TypePair context) => context.DestinationType == typeof(string);
    public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
        => Expression.Call(resolvedSource, ExpressionBuilder.ObjectToString);
}