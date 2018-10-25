using System.IO;

namespace GetGit.Git
{
    public class Env
    {
        public static string ExecutingDirectory()
        {
            var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return SanePath.Normalize(Path.GetDirectoryName(executablePath));
        }
    }
}
