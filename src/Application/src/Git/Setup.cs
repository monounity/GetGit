using log4net;
using System.IO;

namespace GetGit.Git
{
    public class Setup
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Setup));

        public static void InitializeRepo(string path)
        {
            Log.Info("Initializing repo " + path);
            if (Directory.Exists(path))
            {
                Log.Warn("Wiping " + path);
                Obliterate(path);
            }

            Directory.CreateDirectory(path);
            CopyFilesTo(path);
        }

        public static void CopyFilesTo(string path)
        {
            File.Copy(SanePath.Combine(Env.ExecutingDirectory(), "gitignore-template.txt"), SanePath.Combine(path, ".gitignore"));
            File.Copy(SanePath.Combine(Env.ExecutingDirectory(), "gitattributes-template.txt"), SanePath.Combine(path, ".gitattributes"));
        }

        public static void Obliterate(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            var items = directory.GetFileSystemInfos("*", SearchOption.AllDirectories);
            var progress = new Progress<FileSystemInfo>(Log, items, "Setting file attributes,");

            foreach (var item in items)
            {
                item.Attributes = FileAttributes.Normal;
                progress.Report();
            }

            progress.Done("File statuses updated");

            Log.Warn("Deleting " + directory.FullName);
            directory.Delete(true);
        }
    }
}
