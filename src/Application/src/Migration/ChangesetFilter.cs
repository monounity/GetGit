using log4net;
using System.Collections.Generic;
using System.Linq;
using GetGit.Git;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    public class ChangesetFilter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChangesetFilter));

        private readonly GitCommand _git;
        private readonly IEnumerable<int> _excludedChangesets;
        private bool _firstRunAfterInit;

        public ChangesetFilter(GitCommand git, IEnumerable<int> excludedChangesets, bool firstRunAfterInit)
        {
            _git = git;
            _excludedChangesets = excludedChangesets;
            _firstRunAfterInit = firstRunAfterInit;
        }
         
        public bool Skip(IChangeset changeset, IEnumerable<IChange> changes, IEnumerable<BranchMapping> mappings)
        {
            if (_firstRunAfterInit)
            {
                _firstRunAfterInit = false;
                _git.Commit(changeset);
                return true;
            }

            if (_excludedChangesets.Contains(changeset.ChangesetId))
            {
                Log.Warn("Skipping C" + changeset.ChangesetId);
                return true;
            }

            if (!_firstRunAfterInit && changes.All(c => c.Item.ItemType == ItemType.Folder) && changes.All(c => c.ChangeType.HasFlag(ChangeType.Add)))
            {
                Log.Warn("Only folders in changeset C" + changeset.ChangesetId + ", skipping: " + changes.ElementAt(0).Item.ServerItem);
                return true;
            }

            if (!_firstRunAfterInit && !mappings.Any())
            {
                Log.Warn("No branch found for C" + changeset.ChangesetId + ", first file: " + changes.ElementAt(0).Item.ServerItem);
                return true;
            }

            if (mappings.Any(m => m.Suspicious))
            {
                foreach (var mapping in mappings)
                {
                    Log.Warn("Suspicious branch mapping: " + mapping);
                }

                Log.Warn("This changeset should be inspected before applying any changes");
                Log.Warn("Press S to skip this changeset (and continue from C" + (changeset.ChangesetId + 1) + ") or press any key to apply the changes (only if you know what you are doing!!)");

                if (System.Console.ReadKey().Key == System.ConsoleKey.S)
                {
                    Log.Warn("Skipping C" + changeset.ChangesetId);
                    return true;
                }
            }

            return false;
        }
    }
}
