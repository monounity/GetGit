using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs.Api.Interface;
using tfs = Microsoft.TeamFoundation.VersionControl.Client;

namespace GetGit.Tfs.Api.Implementation
{
    [ExcludeFromCodeCoverage]
    public class MergeSource : IMergeSource
    {
        private readonly tfs.MergeSource _mergeSource;

        public MergeSource(tfs.MergeSource mergeSource)
        {
            _mergeSource = mergeSource;
        }

        public bool IsRename => _mergeSource.IsRename;
        public string ServerItem => _mergeSource.ServerItem;
        public int VersionFrom => _mergeSource.VersionFrom;
        public int VersionTo => _mergeSource.VersionTo;
    }
}
