using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class AddFileTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/Main", RepoPath)
                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/SubFolderA/AliceA.txt", "Alice A")
                .Add("$/Mock/Main/AliceB.txt", "Alice B")
                .Add("$/Mock/Main/AliceC.txt", "Alice C")
                .End();
        }

        [Test]
        public void HistoryWalker_should_add_files()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files");

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(5));
            Assert.That(result.Stdout.Contains(".gitattributes"));
            Assert.That(result.Stdout.Contains(".gitignore"));
            Assert.That(result.Stdout.Contains("AliceB.txt"));
            Assert.That(result.Stdout.Contains("AliceC.txt"));
            Assert.That(result.Stdout.Contains("SubFolderA/AliceA.txt"));
        }
    }
}
