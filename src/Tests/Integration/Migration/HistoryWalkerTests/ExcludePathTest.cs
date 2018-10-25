using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class ExcludePathTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Exclude(new Regex(@"AliceB", RegexOptions.IgnoreCase))
                .Exclude(new Regex(@"/SubFolder", RegexOptions.IgnoreCase))
                .Branch("master", "$/Mock/Main", RepoPath)
                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceA.txt", "Alice A")
                .Add("$/Mock/Main/AliceB.txt", "Alice B")
                .Add("$/Mock/Main/SubFolder/.gitignore", "*.dll")
                .End();
        }

        [Test]
        public void HistoryWalker_should_exclude_files_matching_regex_patterns()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(3));
            Assert.That(result.Stdout.Contains(".gitattributes"));
            Assert.That(result.Stdout.Contains(".gitignore"));
            Assert.That(result.Stdout.Contains("AliceA.txt"));
        }
    }
}
