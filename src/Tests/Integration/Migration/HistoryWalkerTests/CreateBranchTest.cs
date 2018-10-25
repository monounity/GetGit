using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class CreateBranchTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
        }

        [Test]
        public void HistoryWalker_should_create_a_new_branch_from_the_parent_branch()
        {
            Env
                .Branch("master", "$/Mock/Main", RepoPath)
                .Branch("feature-branch-x", "$/Mock/FeatureX", RepoPath)
                .Branch("feature-branch-y", "$/Mock/FeatureY", RepoPath)

                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceMaster.txt", "Alice Master")

                .Changeset(OldUserAlice, UserAlice, "Branch from master to X")
                .Hierarchy("$/Mock/Main/", "$/Mock/FeatureX/")
                .Add("$/Mock/FeatureX/AliceX.txt", "Alice X")

                .Changeset(OldUserAlice, UserAlice, "Branch from master to Y")
                .Hierarchy("$/Mock/Main/", "$/Mock/FeatureY/")
                .Add("$/Mock/FeatureY/AliceY.txt", "Alice Y")

                .End();

            HistoryWalker.Walk(new EntryPoint(1));

            var branches = GitProcess.Execute("for-each-ref --format='%(refname:short)' refs/heads");
            Assert.That(branches.Stderr.Count(), Is.EqualTo(0));
            Assert.That(branches.Stdout.Count(), Is.EqualTo(3));
            Assert.That(branches.Stdout.Contains("'master'"));
            Assert.That(branches.Stdout.Contains("'feature-branch-x'"));
            Assert.That(branches.Stdout.Contains("'feature-branch-y'"));

            Assert.That(Git.CurrentBranch(), Is.EqualTo("feature-branch-y"));

            var commits = GitProcess.Execute("log --pretty=format:'%s'");
            Assert.That(commits.Stderr.Count(), Is.EqualTo(0));
            Assert.That(commits.Stdout.Count(), Is.EqualTo(2));
            Assert.That(commits.Stdout.Contains("'[C3] Branch from master to Y'"));
            Assert.That(commits.Stdout.Contains("'[C1] Add files'"));
        }
    }
}
