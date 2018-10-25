using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GetGit.Tfs.Api.Interface;
using tfs = Microsoft.TeamFoundation.VersionControl.Client;

namespace GetGit.Tfs.Api.Implementation
{
    [ExcludeFromCodeCoverage]
    public class Change : IChange
    {
        private readonly tfs.Change _change;

        public Change(tfs.Change change)
        {
            _change = change;

            if (change.ChangeType.HasFlag(tfs.ChangeType.Rename) &&
                change.Item.ItemType == tfs.ItemType.File &&
                change.MergeSources != null &&
                change.MergeSources.Any())
            {
                Rename = new MergeSource(_change.MergeSources.ElementAt(0));
            }
        }

        public ChangeType ChangeType => (ChangeType)_change.ChangeType;
        public IItem Item => new Item(_change.Item);
        public IEnumerable<IMergeSource> MergeSources => _change.MergeSources.Select(m => new MergeSource(m));
        public IMergeSource Rename { get; }

        public override string ToString()
        {
            return ChangeType.ToString();
        }
    }
}
