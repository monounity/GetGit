using System;

namespace GetGit.Tfs.Api.Interface
{
    [Flags]
    public enum ItemType
    {
        Any = 0,
        Folder = 1,
        File = 2
    }
}
