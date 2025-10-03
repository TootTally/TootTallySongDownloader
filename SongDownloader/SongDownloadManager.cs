using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Core;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TootTallyCore.APIServices;
using TootTallyCore.Graphics.ProgressCounters;
using TootTallyCore.Utils.Helpers;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallySongDownloader.Ui;
using TrombLoader.CustomTracks;
using static TootTallyCore.APIServices.SerializableClass;

namespace TootTallySongDownloader.SongDownloader
{
    public static class SongDownloadManager
    {
        private static bool _isInitialized;
        private static List<string> _newDownloadedTrackRefs;
        private static List<string> _deletedTrackRefs;
        private static ConcurrentQueue<QueuedSongData> _downloadQueue;
        private static List<QueuedSongData> _currentDownloads;

        public static void Init()
        {
            if (_isInitialized) return;

            _newDownloadedTrackRefs = new List<string>();
            _deletedTrackRefs = new List<string>();
            _downloadQueue = new ConcurrentQueue<QueuedSongData>();
            _currentDownloads = new List<QueuedSongData>();
        }

        public static void ReloadSongIfNewSongDownloaded()
        {
            if (_newDownloadedTrackRefs.Count == 0 && _deletedTrackRefs.Count == 0) return;

            TootTallyNotifManager.DisplayNotif("Reloading songs...");
            _newDownloadedTrackRefs.Clear();
            _deletedTrackRefs.Clear();
            TootTallyCore.Plugin.Instance.reloadManager.ReloadAll(new ProgressCallbacks
            {
                onComplete = delegate
                {
                    TootTallyNotifManager.DisplayNotif("Reload complete!");
                },
                onError = err =>
                {
                    TootTallyNotifManager.DisplayNotif($"Reloading failed! {err.Message}");
                }
            });
        }

        public static void AddToQueue(string link, ProgressCounter progress, SongDownloadObject songObj)
        {
            var songData = new QueuedSongData(link, progress, songObj);

            if (_currentDownloads.Count < 4)
            {
                StartDownload(songData);
                return;
            }

            _downloadQueue.Enqueue(songData);
        }

        public static void StartDownload(QueuedSongData songData)
        {
            _currentDownloads.Add(songData);

            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadZipFromServer(
                songData.link,
                songData.progress,
                data =>
                {
                    OnDownloadFinish(songData);

                    if (data == null)
                    {
                        TootTallyNotifManager.DisplayNotif("Download failed.");
                        songData.songObj.OnSongDownload(false);
                        return;
                    }

                    var downloadDir = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location)!, "Downloads/");
                    var fileName = $"{songData.songObj.GetSong.id}.zip";

                    try
                    {
                        if (!Directory.Exists(downloadDir))
                            Directory.CreateDirectory(downloadDir);
                    }
                    catch (IOException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("IO error creating download directory");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Insufficient permissions while creating download directory");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Unknown error creating download directory (check logs!)");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }

                    try
                    {
                        FileHelper.WriteBytesToFile(downloadDir, fileName, data);
                    }
                    catch (IOException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("IO error writing ZIP archive");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Insufficient permissions while writing ZIP archive");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Unknown error writing ZIP archive (check logs!)");
                        songData.songObj.OnSongDownload(false);
                        return;
                    }

                    var source = Path.Combine(downloadDir, fileName);
                    var destination = Path.Combine(Paths.BepInExRootPath, "CustomSongs/");

                    try
                    {
                        FileHelper.ExtractZipToDirectory(source, destination);
                    }
                    catch (InvalidDataException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Downloaded file was not a ZIP archive");
                        songData.songObj.OnSongDownload(false);
                        return;
                    }
                    catch (IOException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("IO error extracting ZIP archive");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Insufficient permissions while extracting ZIP archive");
                        songData.songObj.OnSongDownload(false);

                        return;
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError(e.ToString());

                        TootTallyNotifManager.DisplayNotif("Unknown error extracting ZIP archive (check logs!)");
                        songData.songObj.OnSongDownload(false);
                        return;
                    }
                    finally
                    {
                        FileHelper.DeleteFile(downloadDir, fileName);
                    }
                    songData.songObj.OnSongDownload();
                }
            ));
        }

        public static void OnDownloadFinish(QueuedSongData obj)
        {
            _currentDownloads.Remove(obj);

            if (_downloadQueue.Count > 0 && _currentDownloads.Count < 4)
            {
                _downloadQueue.TryDequeue(out QueuedSongData queuedObj);
                StartDownload(queuedObj);
            }
        }

        internal static void AddTrackRefToDownloadedSong(string trackref)
        {
            _newDownloadedTrackRefs.Add(trackref);
            _deletedTrackRefs.Remove(trackref);
        }

        internal static void MarkTrackDeleted(string trackref)
        {
            _deletedTrackRefs.Add(trackref);
            _newDownloadedTrackRefs.Remove(trackref);
        }

        public static bool IsTrackDeletable(string trackRef)
        {
            var trackOpt = TrackLookup.tryLookup(trackRef);
            return FSharpOption<TromboneTrack>.get_IsSome(trackOpt)
                   && trackOpt.Value is CustomTrack
                   && !WasTrackDeleted(trackRef);
        }

        public static bool HasTrackDownloaded(string trackRef)
        {
            return (FSharpOption<TromboneTrack>.get_IsSome(TrackLookup.tryLookup(trackRef)) && !WasTrackDeleted(trackRef))
                   || IsAlreadyDownloaded(trackRef);
        }

        public static bool IsAlreadyDownloaded(string trackref) => _newDownloadedTrackRefs.Contains(trackref);
        public static bool WasTrackDeleted(string trackref) => _deletedTrackRefs.Contains(trackref);

        public class QueuedSongData(string link, ProgressCounter progress, SongDownloadObject songObj)
        {
            public string link = link;
            public ProgressCounter progress = progress;
            public SongDownloadObject songObj = songObj;
        }
    }
}
