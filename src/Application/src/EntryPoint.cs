using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs;

namespace GetGit
{
    public class EntryPoint
    {
        public EntryPoint(int versionFrom) :
            this("", new Version(versionFrom), new Version())
        { }

        public EntryPoint(int? versionFrom = null, int? versionTo = null) :
            this("", new Version(versionFrom), new Version(versionTo))
        { }

        public EntryPoint(string path, int? versionFrom = null, int? versionTo = null) :
            this(path, new Version(versionFrom), new Version(versionTo))
        { }

        public EntryPoint(string path, Version versionFrom = null, Version versionTo = null)
        {
            Path = path;
            VersionFrom = versionFrom ?? new Version();
            VersionTo = versionTo ?? new Version();
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return (Path + " [" + VersionFrom + "] [" + VersionTo + "]").Trim();
        }

        public string Path { get; }
        public Version VersionFrom { get; }
        public Version VersionTo { get; }
    }
}
