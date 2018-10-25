using System.IO;

namespace GetGit
{
    public static class SanePath
    {
        public static string Combine(params string[] paths)
        {
            var combined = "";

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];

                if (path.Length == 0)
                {
                    continue;
                }

                var c = path[path.Length - 1];

                if (i > 0 && c != '/' && c != '\\' && c != ':')
                {
                    combined += Path.AltDirectorySeparatorChar;
                }

                combined += path;
            }

            return Normalize(combined);
        }

        public static string Normalize(string path)
        {
            return path
                .Replace("\\", "/")
                .Replace("//", "/")
                .Replace("//", "/");
        }
    }
}
