using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    public class Branch
    {
        public Branch(BranchMapping mapping)
        {
            Mapping = mapping;
        }

        public Branch(BranchMapping mapping, BranchMapping parent, BranchMapping mergeFrom, IDictionary<string, IItem> paths, IEnumerable<IChange> changes)
        {
            Mapping = mapping;
            Parent = parent;
            MergeFrom = mergeFrom;
            Paths = paths;
            Changes = changes;
        }

        public BranchMapping Mapping { get; }
        public BranchMapping Parent { get; }
        public BranchMapping MergeFrom { get; }
        public IDictionary<string, IItem> Paths { get; }
        public IEnumerable<IChange> Changes { get; }
        public bool HasMerge => MergeFrom != null;

        public bool Orphan()
        {
            return Parent == null && Mapping.Name != "master";
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "Local branch: " + Mapping + (MergeFrom == null ? "": ", MergeFrom: " + MergeFrom);
        }
    }
}
