using System;
using System.Diagnostics.CodeAnalysis;

namespace GetGit.Migration
{
    [ExcludeFromCodeCoverage]
    public class NetshCommand
    {
        private readonly NetshProcess _netshProcess;

        public NetshCommand(NetshProcess netshProcess)
        {
            _netshProcess = netshProcess;
        }

        public void ConfigureAvailablePorts()
        {
            // Avoid port exhaustion when downloading multiple files in parallel.
            // These values will be restored to default values after rebooting the os.
            var arguments = "int ipv4 set dynamicport tcp start=32768 num=32768";
            var result = _netshProcess.Execute(arguments);

            if (result != 0)
            {
                throw new Exception($"Unable to configure available ports [netsh {arguments}]");
            }
        }

        public void Status()
        {
            _netshProcess.Execute("int ipv4 show dynamicport tcp");
        }
    }
}
