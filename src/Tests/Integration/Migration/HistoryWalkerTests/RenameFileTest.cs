using GetGit;
using GetGit.Migration;
using GetGit.Tfs.Api.Interface;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class RenameFileTest : BaseTest
    {
        private string _aliceAContent = GenerateRandomString(200);
        private string _aliceBContent = GenerateRandomString(200);
        private string _aliceCContent = GenerateRandomString(200);
        private string _aliceDContent = GenerateRandomString(200);
        private string _aliceEContent = GenerateRandomString(200);

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Exclude(new Regex(@"/SubFolder", RegexOptions.IgnoreCase))
                .Branch("master", "$/Mock/Main", RepoPath)

                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceA1.txt", _aliceAContent)
                .Add("$/Mock/Main/AliceB1.txt", _aliceBContent)
                .Add("$/Mock/Main/AliceC1.txt", _aliceCContent)
                .Add("$/Mock/Main/AliceD1.txt", _aliceDContent)
                .Add("$/Mock/Main/AliceE1.txt", _aliceEContent)

                .Changeset(OldUserAlice, UserAlice, "Edit files")

                .Delete("$/Mock/Main/AliceA1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/AliceA1.txt", "$/Mock/Main/AliceA2.txt", _aliceAContent, ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .Delete("$/Mock/Main/AliceB1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/AliceB1.txt", "$/Mock/Main/AliceB2.txt", _aliceBContent + "xxx", ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .Delete("$/Mock/Main/AliceC1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/AliceC1.txt", "$/Mock/Main/alicec1.txt", _aliceCContent, ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .Delete("$/Mock/Main/AliceD1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/AliceD1.txt", "$/Mock/Main/AliceD1.txt", _aliceDContent, ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .Delete("$/Mock/Main/AliceE1.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/AliceE1.txt", "$/Mock/Main/SubFolder/AliceE1.txt", _aliceDContent, ItemType.File, ChangeType.Rename | ChangeType.Edit)

                .Delete("$/Mock/Main/NotTrackedByGit.txt", ItemType.File, ChangeType.Delete | ChangeType.SourceRename)
                .Rename("$/Mock/Main/NotTrackedByGit.txt", "$/Mock/Main/ShouldNotBeRenamed.txt", "Should not be added", ItemType.File, ChangeType.Rename | ChangeType.Edit)
                .PopItem()

                .End();

            HistoryWalker.Walk(new EntryPoint(1));
        }

        [Test]
        public void HistoryWalker_should_rename()
        {
            var result = GitProcess.Execute("ls-files", false);

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(6));
            Assert.That(result.Stdout.Contains("AliceA2.txt"));
            Assert.That(result.Stdout.Contains("AliceD1.txt"));
        }

        [Test]
        public void HistoryWalker_should_rename_and_edit()
        {
            var result = GitProcess.Execute("ls-files", false);
            Assert.That(result.Stdout.Contains("AliceB2.txt"), Is.True);
            Assert.That(File.ReadAllText(SanePath.Combine(RepoPath, "AliceB2.txt")), Is.EqualTo(_aliceBContent + "xxx"));
        }

        [Test]
        public void HistoryWalker_should_rename_case_sensitive()
        {
            var result = GitProcess.Execute("ls-files", false);
            Assert.That(result.Stdout.Contains("alicec1.txt"));
        }
    }
}
