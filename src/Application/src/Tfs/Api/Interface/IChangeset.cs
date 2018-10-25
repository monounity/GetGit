using System;

namespace GetGit.Tfs.Api.Interface
{
    public interface IChangeset
    {
        Version Version { get; }
        int ChangesetId { get; }
        DateTime CreationDate { get; }
        string CommitterDisplayName { get; }
        string Committer { get; }
        string Comment { get; }
    }
}
