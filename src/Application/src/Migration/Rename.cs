using System.Diagnostics.CodeAnalysis;

namespace GetGit.Migration
{
    public class Rename
    {
        public Rename(string oldPath, string newPath)
        {
            OldPath = oldPath;
            NewPath = newPath;
        }

        public string OldPath { get; }
        public string NewPath { get; }

        public bool Valid()
        {
            return 
                OldPath != NewPath &&
                !string.IsNullOrWhiteSpace(OldPath) &&
                !string.IsNullOrWhiteSpace(NewPath);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return OldPath + " -> " + NewPath;
        }
    }
}
