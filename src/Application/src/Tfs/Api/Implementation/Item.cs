using System.Diagnostics.CodeAnalysis;
using System.IO;
using GetGit.Tfs.Api.Interface;
using tfs = Microsoft.TeamFoundation.VersionControl.Client;

namespace GetGit.Tfs.Api.Implementation
{
    [ExcludeFromCodeCoverage]
    public class Item : IItem
    {
        private readonly tfs.Item _item;

        public Item(tfs.Item item)
        {
            _item = item;
        }

        public ItemType ItemType => (ItemType)_item.ItemType;
        public int ChangesetId => _item.ChangesetId;
        public string ServerItem => _item.ServerItem;
        public byte[] HashValue => _item.HashValue;

        public Stream DownloadFile()
        {
            return _item.DownloadFile();
        }

        public void DownloadFile(string localFileName)
        {
            _item.DownloadFile(localFileName);
        }

        public override string ToString()
        {
            return ItemType + " " + ServerItem;
        }
    }
}
