using GetGit;
using GetGit.Migration;
using GetGit.Tfs.Api.Interface;
using NUnit.Framework;
using System.Linq;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class DeleteFileTest : BaseTest
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            Env
                .Branch("master", "$/Mock/Main", RepoPath)

                .Changeset(OldUserAlice, UserAlice, "Add files")
                .Add("$/Mock/Main/AliceA.txt", "Alice A1")
                .Add("$/Mock/Main/AliceB.txt", "Alice B1")
                .Add("$/Mock/Main/AliceC.txt", "Alice C1")

                .Changeset(OldUserAlice, UserAlice, "Delete files")
                .Item("$/Mock/Main/AliceA.txt", "Alice A1", ItemType.File)
                .Delete("$/Mock/Main/AliceB.txt")
                .Delete("$/Mock/Main/AliceC.txt")

                .End();
        }

        [Test]
        public void HistoryWalker_should_delete_files()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            var result = GitProcess.Execute("ls-files", false);

            Assert.That(result.Stderr.Count(), Is.EqualTo(0));
            Assert.That(result.Stdout.Count(), Is.EqualTo(3));
            Assert.That(result.Stdout.Contains(".gitattributes"));
            Assert.That(result.Stdout.Contains(".gitignore"));
            Assert.That(result.Stdout.Contains("AliceA.txt"));
        }
    }
}
