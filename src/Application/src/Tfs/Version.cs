using System.Diagnostics.CodeAnalysis;
using GetGit.Tfs.Api.Interface;
using tfs = Microsoft.TeamFoundation.VersionControl.Client;

namespace GetGit.Tfs
{
    public class Version
    {
        public Version() { }

        public Version(int? value)
        {
            Value = value;           
        }

        public Version(string value)
        {
            Value = int.Parse(value.Replace("C", ""));
        }

        public Version(IChangeset changeset)
        {
            Value = changeset.ChangesetId;
        }

        public int? Value { get; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Value.HasValue ? "C" + Value : null;
        }

        public tfs.VersionSpec Spec()
        {
            return Value.HasValue ? tfs.VersionSpec.ParseSingleSpec(ToString(), null) : null;
        }
    }
}
