using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class NoBranchTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/Main", RepoPath)
                .Changeset(OldUserAlice, UserAlice, "Add folders")
                .Add("$/Mock/Feature/SubFolderA/AliceA.txt", "Alice A")
                .End();
        }

        [Test]
        public void HistoryWalker_should_skip_changesets_with_no_branch()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(0));
        }
    }
}
