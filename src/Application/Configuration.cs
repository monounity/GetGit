using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GetGit.Migration;

namespace GetGit
{
    [ExcludeFromCodeCoverage]
    public class Configuration
    {
        public const string TfsServerUri = "http://HOST:8080/tfs/COLLECTION";
        public const string TfsProject = "ProjectX";
        public const string GitRepoPath = "C:/ProjectX";
        public const string GitOrigin = "https://example.com/project/x.git";
        public const string UserListingBaseHostName = "example.com";
        public const bool ListUsers = true;
        public const bool InitializeRepo = true;
        public static readonly EntryPoint EntryPoint = new EntryPoint(1);

        public static readonly string[] GitConfig =
        {
            "core.ignorecase true",
            "core.autocrlf true",
            "core.safecrlf false"
        };
 
        public static readonly Regex[] ExcludedPaths =
        {
            new Regex(@"^\$", RegexOptions.IgnoreCase),
            new Regex(@"/obj/", RegexOptions.IgnoreCase),

            new Regex(@"node_modules", RegexOptions.IgnoreCase),
            new Regex(@"/packages/", RegexOptions.IgnoreCase),

            new Regex(@"\.tfignore$", RegexOptions.IgnoreCase),
            new Regex(@"\.user$", RegexOptions.IgnoreCase),
            new Regex(@"\.vspscc$", RegexOptions.IgnoreCase),
            new Regex(@"\.vssscc$", RegexOptions.IgnoreCase),
        };

        public static readonly int[] ExcludedChangesets = 
        {
            
        };

        public static readonly Dictionary<string, UserMapping> UserMappings = new Dictionary<string, UserMapping>
        {
            { @"DOMAIN\username", new UserMapping("User Name", "user.name@example.com") }
        };

        public static readonly IEnumerable<BranchMapping> BranchMappings = new List<BranchMapping>
        {
            new BranchMapping("master", "$/ProjectX/Main/Sources", SanePath.Combine(GitRepoPath, "src")),

            new BranchMapping("release/1.0.0", "$/ProjectX/Releases/1.0", SanePath.Combine(GitRepoPath, "src")),

            new BranchMapping("master", "$/ProjectX", SanePath.Combine(GitRepoPath, "src"), true) // DANGER!!
        };
    }
}
