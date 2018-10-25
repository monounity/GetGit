namespace GetGit.Tfs.Api.Interface
{
    public interface IMergeSource
    {
        bool IsRename { get; }
        string ServerItem { get; }
        int VersionFrom { get; }
        int VersionTo { get; }
    }
}
