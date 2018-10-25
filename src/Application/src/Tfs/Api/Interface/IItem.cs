using System.IO;

namespace GetGit.Tfs.Api.Interface
{
    public interface IItem
    {
        ItemType ItemType { get; }
        int ChangesetId { get; }
        string ServerItem { get; }
        byte[] HashValue { get; }
        void DownloadFile(string localFileName);
        Stream DownloadFile();
    }
}
