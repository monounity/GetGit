using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetGit.Tfs.Api.Interface;

namespace GetGit.Migration
{
    internal class FileDownloader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FileDownloader));

        internal static void Download(ISet<Download> downloads, string message)
        {
            if (!downloads.Any())
            {
                return;
            }

            var progress = new Progress<Download>(Log, downloads, message);

            Parallel.ForEach(
                downloads,
                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                download =>
                {
                    progress.Report();

                    if (download.Item.ItemType == ItemType.File)
                    {
                        Log.Debug("Downloading " + download.Item.ServerItem);

                        var done = false;

                        while (!done)
                        {
                            try
                            {
                                download.Item.DownloadFile(download.Localpath);
                                done = true;
                            }
                            catch (Exception e)
                            {
                                Log.Warn("Error downloading " + download.Item.ServerItem + ": " + e);
                                Thread.Sleep(1000 * 60);
                                Log.Warn("Retrying " + download.Item.ServerItem);
                            }
                        }
                    }
                }
            );

            progress.Done("Downloaded");
        }
    }
}
