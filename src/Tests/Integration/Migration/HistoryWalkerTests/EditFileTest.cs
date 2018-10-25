using GetGit;
using GetGit.Migration;
using NUnit.Framework;
using System.IO;

namespace Tests.Integration.Migration.HistoryWalkerTests
{
    public class EditFileTest : BaseTest
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

                .Changeset(OldUserAlice, UserAlice, "Edit files")
                .Edit("$/Mock/Main/AliceA.txt", "Alice A2")
                .Edit("$/Mock/Main/AliceB.txt", "Alice B2")
                .Edit("$/Mock/Main/AliceC.txt", "Alice C2")

                .End();
        }

        [Test]
        public void HistoryWalker_should_edit_files()
        {
            HistoryWalker.Walk(new EntryPoint(1));

            File.WriteAllText(SanePath.Combine(RepoPath, "AliceA.txt"), "xxx");

            GitProcess.Execute("checkout -- .", false);

            Assert.That(File.ReadAllText(SanePath.Combine(RepoPath, "AliceA.txt")), Is.EqualTo("Alice A2"));
            Assert.That(File.ReadAllText(SanePath.Combine(RepoPath, "AliceB.txt")), Is.EqualTo("Alice B2"));
            Assert.That(File.ReadAllText(SanePath.Combine(RepoPath, "AliceC.txt")), Is.EqualTo("Alice C2"));
        }
    }
}
