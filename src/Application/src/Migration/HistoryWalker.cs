using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GetGit.Git;
using GetGit.Tfs;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    public class HistoryWalker
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HistoryWalker));

        private readonly IVersionControlServer _versionControlServer;
        private readonly GitCommand _git;
        private readonly ChangesetFilter _changesetFilter;
        private readonly IEnumerable<BranchMapping> _branchMappings;
        private readonly IEnumerable<Regex> _excludedPaths;

        public HistoryWalker(IVersionControlServer versionControlServer, GitCommand git, ChangesetFilter changesetFilter, IEnumerable<BranchMapping> branchMappings, IEnumerable<Regex> excludedPaths)
        {
            _versionControlServer = versionControlServer;
            _git = git;
            _changesetFilter = changesetFilter;
            _branchMappings = branchMappings;
            _excludedPaths = excludedPaths;
        }

        public void Walk(EntryPoint entryPoint)
        {
            var tfsHistory = _versionControlServer.GetHistory(entryPoint);
            Walk(tfsHistory);
        }

        private void Walk(IEnumerable<IChangeset> tfsHistory)
        {
            int counter = 0;
            int total = tfsHistory.Count();

            foreach (var changeset in tfsHistory)
            {
                Log.Info((++counter) + "/" + total + " (" + (total - counter) + ") [" + changeset.CommitterDisplayName + "] \"" + changeset.Comment + "\"");

                var allChanges = _versionControlServer.GetChangesForChangeset(changeset.ChangesetId);
                var mappings = ResolveBranchMappings(changeset, allChanges);

                if (_changesetFilter.Skip(changeset, allChanges, mappings))
                {
                    continue;
                }

                var branches = new List<Branch>();
                var filteredChanges = new List<IChange>();

                foreach (var mapping in mappings)
                {
                    Log.Info("Resolved branch mapping: " + mapping);

                    var paths = GetExpectedChangesetPaths(changeset, mapping);
                    var changesForBranch = allChanges.Where(c => mapping.Match(c.Item.ServerItem));

                    if (paths.Count() == 0 && changesForBranch.All(c => c.ChangeType == ChangeType.Delete))
                    {
                        _git.CheckoutBranch("master");
                        _git.Delete(new Branch(mapping));
                        continue;
                    }
                    else
                    {
                        filteredChanges.AddRange(changesForBranch);
                    }

                    var parent = GetParentBranch(changeset, mapping);
                    var mergeFrom = ResolveMergeBranch(changeset, changesForBranch);
                    branches.Add(new Branch(mapping, parent, mergeFrom, paths, changesForBranch));
                }

                if (RenameBranch(branches, filteredChanges))
                {
                    continue;
                }

                Migrate(changeset, branches);
            }
        }

        private IDictionary<string, IItem> GetExpectedChangesetPaths(IChangeset changeset, BranchMapping mapping)
        {
            var itemSet = _versionControlServer.GetItems(mapping.TfsPath, changeset.Version);
            return itemSet.ToDictionary(i => i.ServerItem, i => i);
        }

        private BranchMapping GetParentBranch(IChangeset changeset, BranchMapping mapping)
        {
            if (mapping.Name == "master")
            {
                return null;
            }

            var tfsPath = _versionControlServer.GetBranchParentPath(mapping.TfsPath, changeset.ChangesetId);
            return tfsPath == null ? null : _branchMappings.Where(m => m.Match(tfsPath)).FirstOrDefault();
        }

        private IEnumerable<BranchMapping> ResolveBranchMappings(IChangeset changeset, IEnumerable<IChange> changes)
        {
            ISet<BranchMapping> mappings = new HashSet<BranchMapping>();

            foreach (var mapping in _branchMappings)
            {
                foreach (var change in changes)
                {
                    if (mapping.Match(change))
                    {
                        mappings.Add(mapping);
                    }
                }
            }

            var result = new HashSet<BranchMapping>();

            foreach (var b1 in mappings)
            {
                if (!mappings.Any(b2 => b2.TfsPath.StartsWith(b1.TfsPath) && b1.TfsPath != b2.TfsPath))
                {
                    result.Add(b1);
                }
            }

            return result.Where(m => !m.Excluded);
        }

        private BranchMapping ResolveMergeBranch(IChangeset changeset, IEnumerable<IChange> changes)
        {
            if (!changes.Any(c => c.ChangeType.HasFlag(ChangeType.Merge)))
            {
                return null;
            }

            foreach (var change in changes)
            {
                foreach(var mergeSource in change.MergeSources)
                {
                    if (!mergeSource.IsRename)
                    {
                        foreach(var branch in _branchMappings)
                        {
                            if (branch.Match(mergeSource))
                            {
                                return branch;
                            }
                        }
                    }
                }
            }

            throw new System.Exception("Unable to find merge branch name for " + new Version(changeset));
        }

        private bool RenameBranch(IList<Branch> branches, IList<IChange> filteredChanges)
        {
            if (branches.Count == 2 &&
                branches[0].Mapping.Path == branches[1].Mapping.Path &&
                branches.Any(b => b.Paths.Count == 0) &&
                branches.Any(b => b.Paths.Count == filteredChanges.Count / 2))
            {
                var changeTypes = filteredChanges.Select(c => c.ChangeType).Distinct();

                if (changeTypes.Count() == 2 &&
                    changeTypes.Any(c => c.HasFlag(ChangeType.Rename)) &&
                    changeTypes.Any(c => c.HasFlag(ChangeType.SourceRename)))
                {
                    var from = branches.Where(b => b.Paths.Count == 0).First();
                    var to = branches.Where(b => b.Paths.Count > 0).First();
                    _git.CheckoutBranch("master");
                    _git.Rename(from, to);
                    return true;
                }
            }

            return false;
        }

        private void Migrate(IChangeset changeset, List<Branch> branches)
        {
            foreach (var group in branches.GroupBy(b => new { Branch = b.Mapping.Name, MergeBranch = b.MergeFrom?.Name }))
            {
                var groupMerged = false;

                foreach (var branch in group)
                { 
                    _git.CheckoutBranch(branch);

                    if (branch.HasMerge && !groupMerged)
                    {
                        groupMerged = true;
                        _git.Merge(changeset, branch.MergeFrom.Name);
                        _git.ResetPath(".");
                        _git.CheckoutPath(".");
                        _git.Clean();
                    }

                    Directory.CreateDirectory(branch.Mapping.Path);

                    var delta = new Delta(branch, _excludedPaths, _git);
                    delta.Apply();
                }

                _git.Commit(changeset);
                _git.Clean();
            }
        }
    }
}
