using System;
using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    public class BranchMapping
    {
        public BranchMapping(string name, string tfsPath, string path, bool suspicious = false, bool excluded = false)
        {
            Name = Assert(name, "name");
            TfsPath = AssertTrailingSlash(Assert(tfsPath, "tfs name"));
            Path = AssertTrailingSlash(Assert(path, "path"));
            Suspicious = suspicious;
            Excluded = excluded;
        }

        public string Name { get; }
        public string TfsPath { get; }
        public string Path { get; }
        public bool Suspicious { get; }
        public bool Excluded { get; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Name + " => " + Path + " (" + TfsPath + ")";
        }

        public bool Match(string path)
        {
            return AssertTrailingSlash(path).StartsWith(TfsPath);
        }

        public bool Match(IMergeSource mergeSource)
        {
            return mergeSource.ServerItem.StartsWith(TfsPath);
        }

        public bool Match(IChange change)
        {
            return AssertTrailingSlash(change).StartsWith(TfsPath);
        }

        public string GetLocalPath(string path)
        {
            var localPath = path.Replace(TfsPath, Path);

            if(localPath == path)
            {
                localPath = AssertTrailingSlash(path).Replace(TfsPath, Path);
            }

            return localPath;
        }

        public string GetLocalPath(IChange change)
        {
            var localPath = change.Item.ServerItem.Replace(TfsPath, Path);

            if (localPath == change.Item.ServerItem)
            {
                localPath = AssertTrailingSlash(change).Replace(TfsPath, Path);
            }

            return localPath;
        }

        public string GetLocalPath(IMergeSource mergeSource)
        {
            var localPath = mergeSource.ServerItem.Replace(TfsPath, Path);

            if (localPath == mergeSource.ServerItem)
            {
                localPath = AssertTrailingSlash(mergeSource.ServerItem).Replace(TfsPath, Path);
            }

            return localPath;
        }

        private string AssertTrailingSlash(IChange change)
        {
            return change.Item.ItemType == ItemType.Folder ?
                AssertTrailingSlash(change.Item.ServerItem) :
                change.Item.ServerItem;
        }

        private string AssertTrailingSlash(string path)
        { 
            return path.EndsWith("/") ? path : path + "/";
        }

        private string Assert(string value, string property)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Local branch " + property + " can't be empty");
            }

            return value;
        }
    }
}
