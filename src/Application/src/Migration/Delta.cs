using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GetGit.Git;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    public class Delta
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Delta));

        private readonly Branch _branch;
        private readonly IEnumerable<Regex> _excludedPaths;
        private readonly GitCommand _git;

        private readonly ISet<string> _deletes;
        private readonly IDictionary<string, Rename> _renames;
        private readonly ISet<Download> _adds;
        private readonly ISet<Download> _updates;

        public Delta(Branch branch, IEnumerable<Regex> excludedPaths, GitCommand git)
        {
            _branch = branch;
            _excludedPaths = excludedPaths;
            _git = git;

            _deletes = new HashSet<string>();
            _renames = new Dictionary<string, Rename>();
            _adds = new HashSet<Download>();
            _updates = new HashSet<Download>();
        }

        public void Apply()
        {
            Log.Info($"[{_branch.Mapping.Name}] [{_branch.Mapping.Path}]");
            Calculate();
            Rename();
            Delete();
            Add();
            Update();
        }

        private void Calculate()
        {
            FindRenames();
            FindDeletes();
            FindChanges();
            Log.Info($"Found {_renames.Count} rename(s), {_deletes.Count} delete(s), {_adds.Count} add(s), {_updates.Count} update(s)");
        }

        private void Rename()
        {
            foreach (var localPath in _renames.Keys)
            {
                var rename = _renames[localPath];

                Directory.CreateDirectory(Directory.GetParent(rename.NewPath).FullName);

                if (_git.Move(rename.OldPath, rename.NewPath).ExitCode != 0)
                {
                    Log.Warn("Failed to move " + rename.OldPath + " to " + rename.NewPath);
                }
            }
        }

        private void Delete()
        {
            var progress = new Progress<string>(Log, _deletes, "Deleting");

            foreach (var file in _deletes)
            {
                progress.Report();
                File.Delete(file);
            }

            progress.Done("Deleted");
        }

        private void Add()
        {
            FileDownloader.Download(_adds, "Downloading adds,");
        }

        private void Update()
        {
            FileDownloader.Download(_updates, "Downloading updates,");
        }

        private void FindRenames()
        {
            Log.Info("Looking for files to rename...");

            var trackedFiles = GetTrackedFiles();

            foreach (var change in _branch.Changes.Where(c => c.Rename != null))
            {
                var oldPathKey = _branch.Mapping.GetLocalPath(change.Rename);

                if (!File.Exists(oldPathKey) || !trackedFiles.ContainsKey(oldPathKey.ToLower()))
                {
                    Log.Debug("Renamed file isn't tracked by Git: " + change.Rename.ServerItem);
                    continue;
                }

                var rename = new Rename(trackedFiles[oldPathKey.ToLower()], _branch.Mapping.GetLocalPath(change));

                if (_excludedPaths.Any(r => r.IsMatch(rename.NewPath)))
                {
                    _deletes.Add(rename.OldPath);
                    continue;
                }

                if (!rename.Valid())
                {
                    Log.Warn("Can't rename " + rename.OldPath + " to " + rename.NewPath);
                    continue;
                }

                _renames[oldPathKey.ToLower()] = rename;
                _updates.Add(new Download(change.Item, rename.NewPath));
            }
        }

        private Dictionary<string, string> GetTrackedFiles()
        {
            var lowerTrackedFiles = new Dictionary<string, string>();

            foreach (var trackedFile in _git.TrackedFiles())
            {
                lowerTrackedFiles[trackedFile.ToLower()] = trackedFile;
            }

            return lowerTrackedFiles;
        }

        private void FindDeletes()
        {
            Log.Info("Looking for files to delete...");

            var localPaths = GetLocalPaths();
            var gitDirectory = SanePath.Combine(_git.RepoPath, ".git").ToLower();

            foreach (var file in Directory.GetFiles(_branch.Mapping.Path, "*.*", SearchOption.AllDirectories))
            {
                var exactLocalPath = SanePath.Normalize(file);
                var lowerLocalPath = exactLocalPath.ToLower();

                if (!localPaths.Keys.Contains(lowerLocalPath) && !_renames.ContainsKey(lowerLocalPath) && !lowerLocalPath.Contains(gitDirectory))
                {
                    _deletes.Add(exactLocalPath);
                }
            }
        }

        private Dictionary<string, string> GetLocalPaths()
        {
            var localPaths = new Dictionary<string, string>();

            var exactLocalPaths = _branch.Paths.Keys
                .Select(p => _branch.Mapping.GetLocalPath(p))
                .Where(p => !_excludedPaths.Any(r => r.IsMatch(p)))
                .ToList();

            foreach (var exactLocalPath in exactLocalPaths)
            {
                localPaths[exactLocalPath.ToLower()] = exactLocalPath;
            }

            return localPaths;
        }

        private void FindChanges()
        {
            Log.Info("Looking for files to create or update...");

            foreach (var path in _branch.Paths.Keys)
            {
                var item = _branch.Paths[path];
                var localPath = _branch.Mapping.GetLocalPath(path);

                if (_excludedPaths.Any(r => r.IsMatch(localPath)))
                {
                    continue;
                }

                if (item.ItemType == ItemType.File)
                {
                    if (File.Exists(localPath))
                    {
                        var bytes = File.ReadAllBytes(localPath);
                        var hashValue = Hash.Compute(bytes);
                        if (!Hash.AreEqual(hashValue, item.HashValue))
                        {
                            _updates.Add(new Download(item, localPath));
                        }
                    }
                    else
                    {
                        _adds.Add(new Download(item, localPath));
                    }
                }
            }
        }
    }
}
