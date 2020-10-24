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
        public Type SourceType { get; }
        public Type DestinationType { get; }
        public MemberPath[] MembersToExpand { get; }
        private ICollection<ProjectionRequest> PreviousRequests { get; }
        internal bool AlreadyExists => PreviousRequests.Contains(this);
        public ProjectionRequest(Type sourceType, Type destinationType, MemberPath[] membersToExpand, ICollection<ProjectionRequest> previousRequests)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            MembersToExpand = membersToExpand;
            PreviousRequests = previousRequests;
        }
        
        internal ICollection<ProjectionRequest> GetPreviousRequestsAndSelf() => new HashSet<ProjectionRequest>(PreviousRequests.Concat(new[] { this }));

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

        public static bool operator ==(ProjectionRequest left, ProjectionRequest right) => Equals(left, right);

        public static bool operator !=(ProjectionRequest left, ProjectionRequest right) => !Equals(left, right);

        public bool ShouldExpand(MemberPath currentPath)
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