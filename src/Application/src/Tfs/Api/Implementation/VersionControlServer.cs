using log4net;
using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using GetGit.Tfs.Api.Interface;
using tfs = Microsoft.TeamFoundation.VersionControl.Client;

namespace GetGit.Tfs.Api.Implementation
{
    [ExcludeFromCodeCoverage]
    public class VersionControlServer : IVersionControlServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VersionControlServer));

        private readonly tfs.VersionControlServer _versionControlServer;
        private readonly tfs.TeamProject _teamProject;

        public VersionControlServer(string tfsServerUri, string tfsProject)
        {
            var tfsCollection = new TfsTeamProjectCollection(new Uri(tfsServerUri));
            _versionControlServer = (tfs.VersionControlServer)tfsCollection.GetService(typeof(tfs.VersionControlServer));
            _teamProject = _versionControlServer.GetTeamProject(tfsProject);
        }

        public string GetBranchParentPath(string path, int changesetId)
        {
            var itemSpec = new[] { new tfs.ItemSpec(path, tfs.RecursionType.None) };
            var versionSpec = new tfs.ChangesetVersionSpec(changesetId);
            var branchHistoryTree = _versionControlServer.GetBranchHistory(itemSpec, versionSpec);

            if (branchHistoryTree.Count() > 0 && branchHistoryTree[0].Count() > 0)
            {
                return branchHistoryTree[0][0].Relative.BranchToItem.ServerItem;
            }

            return null;
        }

        public IEnumerable<IChange> GetChangesForChangeset(int changesetId)
        {
            return _versionControlServer
                .GetChangesForChangeset(changesetId, false, Int32.MaxValue, null, null, true)
                .Select(c => new Change(c))
                .ToList();
        }

        public IEnumerable<IChangeset> GetHistory(EntryPoint entryPoint)
        {
            Log.Info("Querying changesets for " + entryPoint);

            var history = _versionControlServer.QueryHistory(
                SanePath.Combine(_teamProject.ServerItem, entryPoint.Path),
                tfs.VersionSpec.Latest,
                0,            // no deletionId
                tfs.RecursionType.Full,
                null,         // any user
                entryPoint.VersionFrom.Spec(),
                entryPoint.VersionTo.Spec(),
                int.MaxValue, // number of items to return
                false,        // don't include individual item changes
                false,        // no slotMode
                false,        // don't include download info
                true          // true: sort ascending, first changeset first
            );

            return history
                .Cast<tfs.Changeset>()
                .Select(c => new Changeset(c));
        }

        public IItem GetItem(string path, Version version)
        {
            return new Item(_versionControlServer.GetItem(path, version.Spec()));
        }

        public IEnumerable<IItem> GetItems(string path, Version version)
        {
            var itemSet = _versionControlServer.GetItems(path, version.Spec(), tfs.RecursionType.Full);
            return itemSet.Items.Select(i => new Item(i));
        }

        public void ListUsers(EntryPoint entryPoint, string hostname)
        {
            var tfsHistory = GetHistory(entryPoint);
            var cache = new Dictionary<string, string>();

            foreach (var changeset in tfsHistory)
            {
                if (!cache.ContainsKey(changeset.Committer))
                {
                    cache.Add(changeset.Committer, changeset.CommitterDisplayName);
                }
            }

            var usernames = cache.Keys.ToList();
            usernames.Sort();

            foreach (var username in usernames)
            {
                var name = cache[username];
                var email = $"{ReplaceDiacritics(name.ToLower())}@{hostname}".Replace(' ', '.');

                Console.WriteLine("{ @\"" + username + "\", new UserMapping(\"" + name + "\", \"" + email + "\") },");
            }
        }

        private static string ReplaceDiacritics(string s)
        {
            var result = "";

            foreach (var c in s.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    result += c;
                }
            }

            return result;
        }
    }
}
