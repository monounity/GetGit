using GetGit;
using GetGit.Migration;
using GetGit.Tfs;
using GetGit.Tfs.Api.Interface;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.Integration
{
    public class MockEnvironment
    {
        private int _changesetCounter = 0;

        private Mock<IChangeset> _changeset;
        private Mock<IChange> _change;
        private Mock<IItem> _item;

        private IList<IChange> _changes;
        private IList<IItem> _items;

        public MockEnvironment()
        {
            VersionControlServer = new Mock<IVersionControlServer>();
            BranchMappings = new List<BranchMapping>();
            Changesets = new List<IChangeset>();
            ExcludedPaths = new List<Regex>();
            ExcludedChangesets = new List<int>();

            VersionControlServer
                .Setup(vcs => vcs.GetHistory(It.IsAny<EntryPoint>()))
                .Returns((EntryPoint entryPoint) => Changesets.Where(c => c.ChangesetId >= entryPoint.VersionFrom.Value));
        }

        public Mock<IVersionControlServer> VersionControlServer { get; }
        public IList<BranchMapping> BranchMappings { get; }
        public IList<IChangeset> Changesets { get; }
        public IList<Regex> ExcludedPaths { get; }
        public IList<int> ExcludedChangesets { get; }

        public MockEnvironment Branch(string name, string tfsPath, string path, bool suspicious = false, bool excluded = false)
        {
            BranchMappings.Add(new BranchMapping(name, tfsPath, path, suspicious, excluded));
            return this;
        }

        public MockEnvironment Changeset(string comitter, string committerDisplayName, string comment)
        {
            End();

            _changesetCounter++;
            _changes = new List<IChange>();
            _items = new List<IItem>();

            _changeset = new Mock<IChangeset>();
            _changeset.Setup(c => c.ChangesetId).Returns(_changesetCounter);
            _changeset.Setup(c => c.Comment).Returns($"[C{_changesetCounter}] {comment}");
            _changeset.Setup(c => c.Committer).Returns(comitter);
            _changeset.Setup(c => c.CommitterDisplayName).Returns(committerDisplayName);
            _changeset.Setup(c => c.CreationDate).Returns(System.DateTime.Now);
            _changeset.Setup(c => c.Version).Returns(new Version(_changesetCounter));

            Changesets.Add(_changeset.Object);

            return this;
        }

        public MockEnvironment Hierarchy(string parentPath, string childPath)
        {
            if (_changeset == null)
            {
                throw new System.Exception("A changeset must be added before setting up a branch hierarchy");
            }

            VersionControlServer
                .Setup(vcs => vcs.GetBranchParentPath(childPath, _changeset.Object.ChangesetId))
                .Returns(parentPath);
            return this;
        }

        public MockEnvironment Merge(string oldTfsPath)
        {
            var mergeSource = new Mock<IMergeSource>();
            mergeSource.Setup(m => m.IsRename).Returns(false);
            mergeSource.Setup(m => m.ServerItem).Returns(oldTfsPath);
            _change.Setup(c => c.MergeSources).Returns(new List<IMergeSource> { mergeSource.Object });
            return this;
        }

        public MockEnvironment File(string path, string content)
        {
            Directory.CreateDirectory(Directory.GetParent(path).FullName);
            System.IO.File.WriteAllText(path, content);
            return this;
        }

        public MockEnvironment Add(string tfsPath, string content, ItemType? itemType = null, ChangeType? changeType = null)
        {
            Change(changeType ?? ChangeType.Add, itemType ?? ItemType.File, tfsPath, content);
            return this;
        }

        public MockEnvironment Edit(string tfsPath, string content, ItemType? itemType = null, ChangeType? changeType = null)
        {
            Change(changeType ?? ChangeType.Edit, itemType ?? ItemType.File, tfsPath, content);
            return this;
        }

        public MockEnvironment Rename(string oldTfsPath, string newTfsPath, string content, ItemType itemType, ChangeType? changeType)
        {
            Change(changeType ?? ChangeType.Rename, itemType, newTfsPath, content);

            var mergeSource = new Mock<IMergeSource>();
            mergeSource.Setup(m => m.IsRename).Returns(true);
            mergeSource.Setup(m => m.ServerItem).Returns(oldTfsPath);
            _change.Setup(c => c.Rename).Returns(mergeSource.Object);

            return this;
        }

        public MockEnvironment Delete(string tfsPath, ItemType? itemType = null, ChangeType? changeType = null)
        {
            _item = new Mock<IItem>();
            _item.Setup(i => i.ChangesetId).Returns(_changeset.Object.ChangesetId);
            _item.Setup(i => i.HashValue).Returns(new byte[] { });
            _item.Setup(i => i.ItemType).Returns(itemType ?? ItemType.File);
            _item.Setup(i => i.ServerItem).Returns(tfsPath);

            _change = new Mock<IChange>();
            _change.Setup(c => c.ChangeType).Returns(changeType ?? ChangeType.Delete);
            _change.Setup(c => c.Item).Returns(_item.Object);
            _changes.Add(_change.Object);

            return this;
        }

        public MockEnvironment Change(ChangeType changeType, ItemType itemType, string tfsPath, string content)
        {
            Item(tfsPath, content, itemType);
            _change = new Mock<IChange>();
            _change.Setup(c => c.ChangeType).Returns(changeType);
            _change.Setup(c => c.Item).Returns(_item.Object);
            _changes.Add(_change.Object);
            return this;
        }

        public MockEnvironment Item(string tfsPath, string content, ItemType itemType)
        {
            _item = new Mock<IItem>();
            _item.Setup(i => i.ChangesetId).Returns(_changeset.Object.ChangesetId);
            _item.Setup(i => i.DownloadFile(It.IsAny<string>())).Callback((string localFilename) => File(localFilename, content));
            _item.Setup(i => i.HashValue).Returns(Hash.Compute(content));
            _item.Setup(i => i.ItemType).Returns(itemType);
            _item.Setup(i => i.ServerItem).Returns(tfsPath);
            _items.Add(_item.Object);
            return this;
        }

        public MockEnvironment PopItem()
        {
            if(_items.Count > 0)
            {
                _items.RemoveAt(_items.Count - 1);
            }
            return this;
        }

        public MockEnvironment End()
        {
            if (_changeset != null)
            {
                var changesCopy = _changes.ToList();
                VersionControlServer
                    .Setup(vcs => vcs.GetChangesForChangeset(_changeset.Object.ChangesetId))
                    .Returns(changesCopy);

                var itemsCopy = _items.ToList();
                VersionControlServer
                    .Setup(vcs => vcs.GetItems(It.IsAny<string>(), _changeset.Object.Version))
                    .Returns((string path, Version version) => itemsCopy.Where(i => i.ServerItem.StartsWith(path)));
            }

            return this;
        }

        public MockEnvironment Exclude(Regex path)
        {
            ExcludedPaths.Add(path);
            return this;
        }

        public MockEnvironment Exclude(int changesetId)
        {
            ExcludedChangesets.Add(changesetId);
            return this;
        }
    }
}
