using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class SkipChangesetWithOnlyFoldersTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/Main", RepoPath)
                .Changeset(OldUserAlice, UserAlice, "Add folders")
                .Add("$/Mock/Main/SubFolderA/", "", GetGit.Tfs.Api.Interface.ItemType.Folder)
                .Add("$/Mock/Main/SubFolderB/", "", GetGit.Tfs.Api.Interface.ItemType.Folder)
                .End();
        }

        [Test]
        public void HistoryWalker_should_skip_changesets_with_only_folders()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(0));
        }
    }
}
