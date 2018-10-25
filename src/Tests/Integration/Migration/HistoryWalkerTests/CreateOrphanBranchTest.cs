using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class CreateOrphanBranchTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/Main", RepoPath)
                .Branch("feature-branch", "$/Mock/Feature", RepoPath)

                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceA.txt", "Alice A")
                .Add("$/Mock/Main/AliceB.txt", "Alice B")

                .Changeset(OldUserBob, UserBob, "Add files")
                .Add("$/Mock/Feature/BobA.txt", "Bob A")
                .Add("$/Mock/Feature/BobB.txt", "Bob B")

                .End();
        }

        [Test]
        public void HistoryWalker_should_delete_the_branch_if_the_changeset_only_contains_deletes()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(4));
            Assert.That(result.Stdout.Contains(".gitattributes"));
            Assert.That(result.Stdout.Contains(".gitignore"));
            Assert.That(result.Stdout.Contains("BobA.txt"));
            Assert.That(result.Stdout.Contains("BobB.txt"));
        }
    }
}
