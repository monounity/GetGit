using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    public class Download
    {
        public Download(IItem item, string localpath)
        {
            Item = item;
            Localpath = localpath;
        }

        public IItem Item { get; }
        public string Localpath { get; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Localpath;
        }
    }
}
