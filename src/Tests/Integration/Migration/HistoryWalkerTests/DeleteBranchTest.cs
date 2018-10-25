using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class DeleteBranchTest : BaseTest
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

                .Changeset(OldUserBob, UserBob, "Edit files")
                .Hierarchy("$/Mock/Main/", "$/Mock/Feature/")
                .Add("$/Mock/Feature/AliceA.txt", "Bob A")
                .Add("$/Mock/Feature/AliceB.txt", "Bob B")

                .Changeset(OldUserBob, UserBob, "Delete branch")
                .Delete("$/Mock/Feature/AliceA.txt")
                .Delete("$/Mock/Feature/AliceB.txt")

                .Changeset(OldUserAlice, UserAlice, "Delete all files")
                .Delete("$/Mock/Main/AliceA.txt")
                .Delete("$/Mock/Main/AliceB.txt")

                .End();
        }

        [Test]
        public void HistoryWalker_should_delete_the_branch_if_the_changeset_only_contains_deletes_and_the_branch_is_not_master()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("for-each-ref --format='%(refname:short)' refs/heads");
            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(1));
            Assert.That(result.Stdout.Contains("'master'"));
        }
    }
}
