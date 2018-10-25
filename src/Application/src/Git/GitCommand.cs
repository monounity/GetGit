using log4net;
using System.Linq;
using System.Collections.Generic;
using System;
using GetGit.Migration;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Git
{
    public class GitCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GitCommand));

        private readonly GitProcess _gitProcess;

        public GitCommand(GitProcess gitProcess, string repoPath, IEnumerable<string> config, string origin)
        {
            _gitProcess = gitProcess;
            RepoPath = repoPath;
            Config = config;
            Origin = origin;
        }

        public string RepoPath { get; }
        public IEnumerable<string> Config { get; }
        public string Origin { get; }

        public void Init()
        {
            Setup.InitializeRepo(RepoPath);
            _gitProcess.Execute("init");
            Configure(true);
        }

        public void Configure(bool hard)
        {
            foreach (var config in Config)
            {
                _gitProcess.Execute($"config {config}");
            }

            _gitProcess.Execute(hard ? $"remote add origin  {Origin}" : $"remote set-url origin {Origin}");
            _gitProcess.Execute("config --list");
        }

        public GitProcessResult Commit(IChangeset changeset)
        {
            _gitProcess.Env(changeset.CommitterDisplayName, changeset.Committer, changeset.CreationDate);

            Log.Info("Staging");
            _gitProcess.Execute("add .");

            Log.Info("Committing");
            return _gitProcess.Execute($"commit --message \"{changeset.Comment}\"");
        }

        public GitProcessResult Merge(IChangeset changeset, string branch)
        {
            _gitProcess.Env(changeset.CommitterDisplayName, changeset.Committer, changeset.CreationDate);
            return _gitProcess.Execute($"merge --no-commit --no-ff {branch}");
        }

        public GitProcessResult CheckoutBranch(Branch branch)
        {
            var result = new GitProcessResult(0);
            var currentBranch = CurrentBranch();

            if (branch.Mapping.Name == currentBranch)
            {
                return result;
            }

            if (branch.Parent != null && branch.Parent.Name != currentBranch)
            {
                result = CheckoutBranch(branch.Parent.Name);
                if (result.ExitCode != 0)
                {
                    throw new Exception("Error checking out parent branch " + branch.Parent.Name);
                }
            }

            result = CheckoutBranch(branch.Mapping.Name, branch.Orphan());
            if (result.ExitCode != 0)
            {
                throw new Exception("Error checking out branch " + branch.Mapping.Name);
            }

            return result;
        }

        public GitProcessResult CheckoutBranch(string branch, bool orphan = false)
        {
            if (branch == CurrentBranch())
            {
                return new GitProcessResult(0);
            }

            var exists = _gitProcess.Execute($"show-ref refs/heads/{branch}", false);

            if (exists.ExitCode == 0 && exists.Stdout.Count() == 1)
            {
                return _gitProcess.Execute($"checkout {branch}");
            }
            else
            {
                if (orphan)
                {
                    var result = _gitProcess.Execute($"checkout --orphan {branch}");
                    _gitProcess.Execute($"rm -r -f -- .");
                    Setup.CopyFilesTo(RepoPath);
                    return result;
                }
                else
                {
                    return _gitProcess.Execute($"checkout -b {branch}");
                }
            }
        }

        public GitProcessResult CheckoutPath(string path)
        {
            return _gitProcess.Execute($"checkout -- {path}", false);
        }

        public GitProcessResult ResetPath(string path)
        {
            return _gitProcess.Execute($"reset -- {path}", false);
        }

        public GitProcessResult Delete(Branch branch)
        {
            if (branch.Mapping.Name == "master")
            {
                Log.Warn("The master branch can't be deleted");
                return new GitProcessResult(1);
            }

            return _gitProcess.Execute($"branch -D {branch.Mapping.Name}");
        }

        public GitProcessResult Rename(Branch from, Branch to)
        {
            if (from.Mapping.Name == "master" || to.Mapping.Name == "master")
            {
                Log.Warn("The master branch can't be renamed");
                return new GitProcessResult(1);
            }

            return _gitProcess.Execute($"branch -m {from.Mapping.Name} {to.Mapping.Name}");
        }

        public GitProcessResult Move(string from, string to)
        {
            return _gitProcess.Execute($"mv --force \"{from}\" \"{to}\"");
        }

        public GitProcessResult Clean()
        {
            return _gitProcess.Execute("clean -fdx", false);
        }

        public IEnumerable<string> TrackedFiles()
        {
            return _gitProcess
                .Execute("ls-files", false)
                .Stdout
                .Select(f => SanePath.Combine(RepoPath, f));
        }

        public string CurrentBranch()
        {
            return _gitProcess.Execute("rev-parse --abbrev-ref HEAD", false).Stdout.ElementAt(0);
        }
    }
}
