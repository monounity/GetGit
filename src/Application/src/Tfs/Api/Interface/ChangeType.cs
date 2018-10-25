using System;

namespace GetGit.Tfs.Api.Interface
{
    [Flags]
    public enum ChangeType
    {
        None = 1,
        Add = 2,
        Edit = 4,
        Encoding = 8,
        Rename = 16,
        Delete = 32,
        Undelete = 64,
        Branch = 128,
        Merge = 256,
        Lock = 512,
        Rollback = 1024,
        SourceRename = 2048,
        Property = 8192
    }
}
