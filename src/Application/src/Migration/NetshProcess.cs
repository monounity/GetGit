using log4net;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GetGit.Migration
{
    [ExcludeFromCodeCoverage]
    public class NetshProcess
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NetshProcess));

        public int Execute(string arguments)
        {
            var process = new Process();

            process.StartInfo.FileName = "netsh";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            process.OutputDataReceived += (s, e) =>
            {
                OnDataReceived(Log.Info, e);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                OnDataReceived(Log.Warn, e);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode;
        }

        private void OnDataReceived(Action<object> logger, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                logger(e.Data);
            }
        }
    }
}
