using log4net;
using log4net.Config;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GetGit.Git;
using GetGit.Migration;
using GetGit.Tfs.Api.Implementation;

namespace GetGit
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("./log.config"));

            Log.Info("Initializing");

            try
            {
                var netsh = new NetshCommand(new NetshProcess());
                var versionControlServer = new VersionControlServer(Configuration.TfsServerUri, Configuration.TfsProject);
                var git = new GitCommand(new GitProcess(Configuration.GitRepoPath, Configuration.UserMappings), Configuration.GitRepoPath, Configuration.GitConfig, Configuration.GitOrigin);
                var changesetFilter = new ChangesetFilter(git, Configuration.ExcludedChangesets, Configuration.InitializeRepo);
                var historyWalker = new HistoryWalker(versionControlServer, git, changesetFilter, Configuration.BranchMappings, Configuration.ExcludedPaths);

                if (Configuration.ListUsers)
                {
                    versionControlServer.ListUsers(Configuration.EntryPoint, Configuration.UserListingBaseHostName);
                }
                else
                {
                    if (Configuration.InitializeRepo)
                    {
                        git.Init();
                    }

                    netsh.ConfigureAvailablePorts();
                    netsh.Status();
                    git.Configure(false);

                    historyWalker.Walk(Configuration.EntryPoint);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Log.Info("Done, press any key to exit");

            Console.ReadKey();
        }
    }
}
