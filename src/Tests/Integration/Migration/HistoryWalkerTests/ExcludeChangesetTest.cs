using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class ExcludeChangesetTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Exclude(2)
                .Branch("master", "$/Mock/Main", RepoPath)

                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceA.txt", "Alice A1")

                .Changeset(OldUserBob, UserBob, "Edit files")
                .Edit("$/Mock/Main/AliceA.txt", "Alice A2")

                .End();
        }

        [Test]
        public void HistoryWalker_should_exclude_files_matching_regex_patterns()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("rev-list --all --count");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(1));
            Assert.That(result.Stdout.Contains("1"));
            Assert.That(File.ReadAllText(SanePath.Combine(RepoPath, "AliceA.txt")), Is.EqualTo("Alice A1"));
        }
    }
}
