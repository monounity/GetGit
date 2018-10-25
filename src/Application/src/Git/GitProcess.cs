using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GetGit.Git
{
    public class GitProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GitProcess));

        private string _envName;
        private string _envUser;
        private DateTime _envDate;

        public GitProcess(string repoPath, IDictionary<string, UserMapping> userMappings)
        {
            RepoPath = repoPath;
            UserMappings = userMappings;
        }

        public string RepoPath { get; }
        public IDictionary<string, UserMapping> UserMappings { get; }

        public void Env(string name, string user, DateTime date)
        {
            var userMapping = UserMappings[user];

            if (userMapping == null)
            {
                throw new Exception("Unmapped user: " + user + " " + name);
            }

            _envName = userMapping.Name;
            _envUser = userMapping.Email;
            _envDate = date;
        }

        public GitProcessResult Execute(string arguments, bool print = true)
        {
            var stdout = new List<string>();
            var stderr = new List<string>();
            var process = new Process();

            process.StartInfo.FileName = "git.exe";
            process.StartInfo.WorkingDirectory = RepoPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            process.StartInfo.EnvironmentVariables["GIT_AUTHOR_NAME"] = _envName;
            process.StartInfo.EnvironmentVariables["GIT_AUTHOR_EMAIL"] = _envUser;
            process.StartInfo.EnvironmentVariables["GIT_AUTHOR_DATE"] = _envDate.ToString("yyyy-MM-ddTHH:mm");
            process.StartInfo.EnvironmentVariables["GIT_COMMITTER_NAME"] = _envName;
            process.StartInfo.EnvironmentVariables["GIT_COMMITTER_EMAIL"] = _envUser;
            process.StartInfo.EnvironmentVariables["GIT_COMMITTER_DATE"] = _envDate.ToString("yyyy-MM-ddTHH:mm");

            process.OutputDataReceived += (s, e) =>
            {
                OnDataReceived(Log.Info, e, stdout, print);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                OnDataReceived(Log.Warn, e, stderr, print);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return new GitProcessResult(process.ExitCode, stdout, stderr);
        }

        private void OnDataReceived(Action<object> logger, DataReceivedEventArgs e, IList<string> list, bool print)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            list.Add(e.Data);

            if (print && !e.Data.Contains("create mode") && !e.Data.Contains("delete mode"))
            {
                logger(e.Data);
            }
        }
    }
}
