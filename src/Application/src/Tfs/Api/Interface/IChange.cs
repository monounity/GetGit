using System.Collections.Generic;

namespace GetGit.Tfs.Api.Interface
{
    public interface IChange
    {
        ChangeType ChangeType { get;  }
        IItem Item { get; }
        IEnumerable<IMergeSource> MergeSources { get; }
        IMergeSource Rename { get; }
    }
}
