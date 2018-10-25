using System;
using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs.Api.Interface;
using tfs = Microsoft.TeamFoundation.VersionControl.Client;

namespace GetGit.Tfs.Api.Implementation
{
    [ExcludeFromCodeCoverage]
    public class Changeset : IChangeset
    {
        private readonly tfs.Changeset _changeset;

        public Changeset(tfs.Changeset changeset)
        {
            _changeset = changeset;
            Version = new Version(changeset.ChangesetId);
        }

        public Version Version { get; }
        public int ChangesetId => _changeset.ChangesetId;
        public DateTime CreationDate => _changeset.CreationDate;
        public string CommitterDisplayName => _changeset.CommitterDisplayName;
        public string Committer => _changeset.Committer;
        public string Comment => "[" + Version + "] " + _changeset.Comment.Replace("\"", "\\\"");

        public override string ToString()
        {
            return CreationDate + " [" + CommitterDisplayName + "] " + Comment;
        }
    }
}
