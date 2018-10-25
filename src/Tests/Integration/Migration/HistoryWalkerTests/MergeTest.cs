using GetGit;
using GetGit.Migration;
using GetGit.Tfs.Api.Interface;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class MergeTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            var aliceBContent = GenerateRandomString(200);

            Env
                .Branch("master", "$/Mock/Main/X", SanePath.Combine(RepoPath, "X"))
                .Branch("feature-branch", "$/Mock/Feature/X", SanePath.Combine(RepoPath, "X"))
                .Branch("feature-branch", "$/Mock/Feature/Y", SanePath.Combine(RepoPath, "Y"))

                // Alice creates files on branch 'master'
                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/X/SubFolderA/AliceA.txt", "Alice A")
                .Add("$/Mock/Main/X/AliceB1.txt", aliceBContent)
                .Add("$/Mock/Main/X/AliceC.txt", "Alice C")

                // Bob creates/deletes/edits files on branch 'feature-branch'
                .Changeset(OldUserBob, UserBob, "Edit files")
                .Hierarchy("$/Mock/Main/X/", "$/Mock/Feature/X/")
                .Add("$/Mock/Feature/X/BobA.txt", "Bob A")
                .Add("$/Mock/Feature/X/BobB.txt", "Should be undone in the merge by the changes from the Tfs")
                .Edit("$/Mock/Feature/X/SubFolderA/AliceA.txt", "Alice A, Bob A")
                .Delete("$/Mock/Feature/X/AliceB1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Feature/X/AliceB1.txt", "$/Mock/Feature/X/AliceB2.txt", aliceBContent + "xxx", ItemType.File, ChangeType.Rename | ChangeType.Edit)
                .Delete("$/Mock/Feature/X/AliceC.txt")
                .Add("$/Mock/Feature/Y/BobC.txt", "Bob C")

                .End();

            HistoryWalker.Walk(new EntryPoint(1));

            Env
                // Alice merges 'feature-branch' into 'master'
                .Changeset(OldUserAlice, UserAlice, "Merge feature-branch")
                .File(SanePath.Combine(RepoPath, "Y/AlienB.txt"), "Alien file that does not belong to the changeset")
                .Add("$/Mock/Main/X/BobA.txt", "Bob A", ItemType.File, ChangeType.Merge | ChangeType.Branch)
                .Edit("$/Mock/Main/X/SubFolderA/AliceA.txt", "Alice A", ItemType.File, ChangeType.Merge | ChangeType.Edit)
                .Merge("$/Mock/Feature/X/SubFolderA/AliceA.txt")
                .Delete("$/Mock/Main/X/AliceB1.txt", ItemType.File, ChangeType.Merge | ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/X/AliceB1.txt", "$/Mock/Main/X/AliceB2.txt", aliceBContent + "xxx", ItemType.File, ChangeType.Merge | ChangeType.Rename | ChangeType.Edit)
                .Delete("$/Mock/Main/X/AliceC.txt", ItemType.File, ChangeType.Merge | ChangeType.Delete)
  
                .End();

            HistoryWalker.Walk(new EntryPoint(3));
        }

        [Test]
        public void HistoryWalker_should_not_delete_files_that_belong_to_the_changeset()
        {
            var result = GitProcess.Execute("ls-files", false);

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(5));
            Assert.That(result.Stdout.Contains(".gitattributes"));
            Assert.That(result.Stdout.Contains(".gitignore"));
            Assert.That(result.Stdout.Contains("X/AliceB2.txt"));
            Assert.That(result.Stdout.Contains("X/BobA.txt"));
            Assert.That(result.Stdout.Contains("X/SubFolderA/AliceA.txt"));
        }

        [Test]
        public void HistoryWalker_should_undo_staged_edits_during_merge_and_use_the_content_from_Tfs()
        {
            Assert.That(File.ReadAllText(SanePath.Combine(RepoPath, "X/SubFolderA/AliceA.txt")), Is.EqualTo("Alice A"));
        }

        [Test]
        public void HistoryWalker_should_delete_files_that_doesnt_belong_to_the_changeset()
        {
            Assert.That(!File.Exists(SanePath.Combine(RepoPath, "Y/AlienB.txt")));
            Assert.That(!File.Exists(SanePath.Combine(RepoPath, "Y/BobC.txt")));
        }

        [Test]
        public void Assert_commit_count()
        {
            var result = GitProcess.Execute("rev-list --all --count");
            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(1));
            Assert.That(result.Stdout.Contains("3"));
        }
    }
}
