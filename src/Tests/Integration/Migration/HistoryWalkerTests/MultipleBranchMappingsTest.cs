using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class MultipleBranchMappingsTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/A", SanePath.Combine(RepoPath, "A"))
                .Branch("master", "$/Mock/B", SanePath.Combine(RepoPath, "B"))
                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/A/AliceA.txt", "Alice A")
                .Add("$/Mock/B/AliceB.txt", "Alice B")
                .End();
        }

        [Test]
        public void HistoryWalker_should_add_files_from_multiple_mapped_branches_into_one()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(4));
            Assert.That(result.Stdout.Contains(".gitattributes"));
            Assert.That(result.Stdout.Contains(".gitignore"));
            Assert.That(result.Stdout.Contains("A/AliceA.txt"));
            Assert.That(result.Stdout.Contains("B/AliceB.txt"));
        }
    }
}
