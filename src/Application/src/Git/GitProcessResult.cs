using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GetGit.Git
{
    public class GitProcessResult
    {
        public GitProcessResult(int exitCode, IEnumerable<string> stdout = null, IEnumerable<string> stderr = null)
        {
            ExitCode = exitCode;
            Stdout = stdout ?? new string[] { };
            Stderr = stderr ?? new string[] { };
        }

        public int ExitCode { get; }
        public IEnumerable<string> Stdout { get; }
        public IEnumerable<string> Stderr { get; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "ExitCode " + ExitCode;
        }
    }
}
