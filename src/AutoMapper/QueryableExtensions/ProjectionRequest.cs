using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public readonly struct ProjectionRequest : IEquatable<ProjectionRequest>
    {
        public readonly Type SourceType;
        public readonly Type DestinationType;
        public readonly MemberPath[] MembersToExpand;
        private readonly ICollection<ProjectionRequest> _previousRequests;
        public ProjectionRequest(Type sourceType, Type destinationType, MemberPath[] membersToExpand, ICollection<ProjectionRequest> previousRequests)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            MembersToExpand = membersToExpand;
            _previousRequests = previousRequests;
        }
        internal bool AlreadyExists => _previousRequests.Contains(this);
        internal ICollection<ProjectionRequest> GetPreviousRequestsAndSelf() => new HashSet<ProjectionRequest>(_previousRequests.Concat(new[] { this }));
        public bool Equals(ProjectionRequest other) => SourceType == other.SourceType && DestinationType == other.DestinationType &&
                MembersToExpand.SequenceEqual(other.MembersToExpand);
        public override bool Equals(object obj) => obj is ProjectionRequest request && Equals(request);
        public override int GetHashCode()
        {
            var hashCode = HashCodeCombiner.Combine(SourceType, DestinationType);
            foreach(var member in MembersToExpand)
            {
                hashCode = HashCodeCombiner.CombineCodes(hashCode, member.GetHashCode());
            }
            return hashCode;
        }
        public static bool operator ==(in ProjectionRequest left, in ProjectionRequest right) => Equals(left, right);
        public static bool operator !=(in ProjectionRequest left, in ProjectionRequest right) => !Equals(left, right);
        public bool ShouldExpand(in MemberPath currentPath)
        {
            foreach (var memberToExpand in MembersToExpand)
            {
                if (memberToExpand.StartsWith(currentPath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}