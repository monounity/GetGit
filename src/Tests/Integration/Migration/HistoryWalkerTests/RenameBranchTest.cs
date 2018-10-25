using GetGit;
using GetGit.Migration;
using GetGit.Tfs.Api.Interface;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class RenameBranchTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/Main", RepoPath)
                .Branch("old-branch", "$/Mock/OldBranch", RepoPath)
                .Branch("new-branch", "$/Mock/NewBranch", RepoPath)

                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceA1.txt", "Alice A1")

                .Changeset(OldUserAlice, UserAlice, "Create branch")
                .Hierarchy("$/Mock/Main/", "$/Mock/OldBranch/")
                .Edit("$/Mock/OldBranch/AliceA1.txt", "Alice A2")

                .Changeset(OldUserAlice, UserAlice, "Rename branch")
                .Delete("$/Mock/OldBranch/AliceA1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/OldBranch/AliceA1.txt", "$/Mock/NewBranch/AliceA1.txt", "", ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .Changeset(OldUserAlice, UserAlice, "Rename branch")
                .Delete("$/Mock/Main/AliceA1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/AliceA1.txt", "$/Mock/NewBranch/AliceA1.txt", "", ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .End();
        }

        [Test]
        public void HistoryWalker_should_rename_any_branch_but_master()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("for-each-ref --format='%(refname:short)' refs/heads");
            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(2));
            Assert.That(result.Stdout.Contains("'master'"));
            Assert.That(result.Stdout.Contains("'new-branch'"));
        }
    }
}
