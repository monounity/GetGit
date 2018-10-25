using System.Collections.Generic;

namespace GetGit.Tfs.Api.Interface
{
    public interface IVersionControlServer
    {
        IEnumerable<IChangeset> GetHistory(EntryPoint entryPoint);
        IEnumerable<IChange> GetChangesForChangeset(int changesetId);
        string GetBranchParentPath(string path, int changesetId);
        IEnumerable<IItem> GetItems(string path, Version version);
    }
}
