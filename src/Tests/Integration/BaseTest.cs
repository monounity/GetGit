using GetGit;
using GetGit.Git;
using GetGit.Migration;
using log4net.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Integration
{
    [TestFixture]
    public class BaseTest
    {
        private static readonly Random _random = new Random();

        protected string RepoPath;
        protected GitProcess GitProcess;
        protected GitCommand Git;
        protected IDictionary<string, UserMapping> UserMappings;
        protected MockEnvironment Env;
        protected HistoryWalker HistoryWalker;

        protected string OldUserAlice = @"oldserver\alice";
        protected string UserAlice = "Alice";

        protected string OldUserBob = @"oldserver\bob";
        protected string UserBob = "Bob";

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            XmlConfigurator.Configure(new FileInfo("./log.config"));

            RepoPath = GetGit.Git.Env.ExecutingDirectory() + "/Repo";

            UserMappings = new Dictionary<string, UserMapping>
            {
                { OldUserAlice, new UserMapping(UserAlice, "alice@example.com") },
                { OldUserBob, new UserMapping(UserBob, "bob@example.com") }
            };

            GitProcess = new GitProcess(RepoPath, UserMappings);
            Git = new GitCommand(GitProcess, RepoPath, new string[]{}, "http://host/repo.git");
            Git.Init();
            Env = new MockEnvironment();

            var changesetFilter = new ChangesetFilter(Git, Env.ExcludedChangesets, false);
            HistoryWalker = new HistoryWalker(Env.VersionControlServer.Object, Git, changesetFilter, Env.BranchMappings, Env.ExcludedPaths);
        }

        [OneTimeTearDown]
        protected virtual void OneTimeTearDown()
        {
            Setup.Obliterate(RepoPath);
        }

        protected static string GenerateRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var buffer = new char[100];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = chars[_random.Next(chars.Length)];
            }

            return new string(buffer);
        }
    }
}
