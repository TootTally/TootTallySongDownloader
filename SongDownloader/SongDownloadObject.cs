#nullable enable

using System;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Core;
using System.IO;
using TootTallyCore.APIServices;
using TootTallyCore.Graphics.ProgressCounters;
using TootTallyCore.Utils.Helpers;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallySettings;
using TootTallySongDownloader.SongDownloader;
using TootTallySongDownloader.Ui;
using TrombLoader.CustomTracks;
using UnityEngine;
using static TootTallyCore.APIServices.SerializableClass;

namespace TootTallySongDownloader
{
    internal class SongDownloadObject : BaseTootTallySettingObject
    {
        // Wow, sum types are verbose as heck in C#
        /// <summary>
        /// Info about the song download (filesize, extension)
        /// </summary>
        private abstract record FileDataState
        {
            /// <summary>
            /// File info has not been requested yet (likely because the user had it downloaded already, so there was no
            /// need to request anything)
            /// </summary>
            internal sealed record Unknown : FileDataState;

            /// <summary>
            /// File info request is in-flight
            /// </summary>
            internal sealed record WaitingOnRequest : FileDataState
            {
                internal readonly Coroutine Coroutine;

                public WaitingOnRequest(Coroutine coroutine)
                {
                    Coroutine = coroutine;
                }
            }

            /// <summary>
            /// File info request succeeded
            /// </summary>
            internal sealed record HasData : FileDataState
            {
                internal readonly FileHelper.FileData FileData;

                public HasData(FileHelper.FileData fileData)
                {
                    FileData = fileData;
                }
            }

            /// <summary>
            /// Failed to fetch file info (web request failed)
            /// </summary>
            internal sealed record ErrorFetchingData : FileDataState;
        }

        private readonly SongRow _songRow;
        private readonly SongDataFromDB _song;
        public bool IsOwned;

        public bool IsDownloadAvailable => !IsOwned && _fileData is FileDataState.HasData;

        /// <summary>
        /// Don't set this directly, use <c>SetFileData</c> which updates the UI as well
        /// </summary>
        private FileDataState _fileData = new FileDataState.Unknown();

        public SongDownloadObject(Transform canvasTransform, SongDataFromDB song, TootTallySettingPage page) : base($"Song{song.track_ref}", page)
        {
            _song = song;

            _songRow = SongRow.Create()
                .WithParent(canvasTransform)
                .WithSongId(song.id)
                .WithSongName(song.name)
                .WithArtist(song.author)
                .WithDurationSeconds(song.song_length)
                .WithDifficulty(song.difficulty)
                .WithCharter(song.charter)
                .WithIsRated(song.is_rated)
                .WithIsDeletable(IsTrackDeletable(song.track_ref))
                .OnDownload(() => DownloadChart(DownloadSource.Auto))
                .OnDownloadFromTootTally(() => DownloadChart(DownloadSource.TootTallyMirror))
                .OnDownloadFromAlternative(() => DownloadChart(DownloadSource.Alternate))
                .OnDelete(DeleteChart);

            if (!HasTrackDownloaded(song.track_ref))
            {
                // User does not have the chart downloaded, query some info from the TootTally API
                TryRequestFileData();
            }
            else
            {
                // User already has the song
                _songRow.WithDownloadState(new DownloadState.Owned());
            }
        }

        public void SetActive(bool active) => _songRow.GameObject.SetActive(active);

        public override void Dispose()
        {
            _songRow.Dispose();

            if (_fileData is FileDataState.WaitingOnRequest waitingOnRequest)
            {
                Plugin.Instance.StopCoroutine(waitingOnRequest.Coroutine);
            }
        }

        public void DownloadChart(DownloadSource dlSource)
        {
            var progress = new ProgressCounter();
            _songRow.WithDownloadState(new DownloadState.Downloading { Progress = progress });

            var link = dlSource switch
            {
                DownloadSource.TootTallyMirror => _song.mirror,
                DownloadSource.Alternate => _song.download,
                DownloadSource.Auto => _song.mirror ?? _song.download,
                _ => throw new ArgumentOutOfRangeException(nameof(dlSource), dlSource, null)
            };
            if (string.IsNullOrEmpty(link))
            {
                // Wuh oh
                TootTallyNotifManager.DisplayError("Missing download link");
                return;
            }

            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadZipFromServer(
                link,
                progress,
                data =>
                {
                    if (data != null)
                    {
                        var downloadDir = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location)!, "Downloads/");
                        var fileName = $"{_song.id}.zip";

                        try
                        {
                            if (!Directory.Exists(downloadDir))
                                Directory.CreateDirectory(downloadDir);
                        }
                        catch (IOException e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("IO error creating download directory");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("Insufficient permissions while creating download directory");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (Exception e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("Unknown error creating download directory (check logs!)");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

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
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("Insufficient permissions while writing ZIP archive");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (Exception e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("Unknown error writing ZIP archive (check logs!)");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

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
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (IOException e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("IO error extracting ZIP archive");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("Insufficient permissions while extracting ZIP archive");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        catch (Exception e)
                        {
                            Plugin.LogError(e.ToString());

                            TootTallyNotifManager.DisplayNotif("Unknown error extracting ZIP archive (check logs!)");
                            _songRow.WithDownloadState(new DownloadState.DownloadAvailable());

                            return;
                        }
                        finally
                        {
                            FileHelper.DeleteFile(downloadDir, fileName);
                        }

                        IsOwned = true;

                        _songRow.WithDownloadState(new DownloadState.Owned());

                        var page = (SongDownloadPage)_page;
                        page.AddTrackRefToDownloadedSong(_song.track_ref);
                        SetActive(!page.ShowNotOwnedOnly);
                    }
                    else
                    {
                        TootTallyNotifManager.DisplayNotif("Download failed.");
                        _songRow.WithDownloadState(new DownloadState.DownloadAvailable());
                    }
                }
            ));
        }

        /// <summary>
        /// Get the directory a chart resides in, for downloaded TrombLoader tracks
        /// </summary>
        /// <returns>The directory, or <c>null</c> if the track is not loaded by TrombLoader</returns>
        private string? GetLocalChartDirectory()
        {
            var trackOpt = TrackLookup.tryLookup(_song.track_ref);
            if (FSharpOption<TromboneTrack>.get_IsNone(trackOpt))
            {
                return null;
            }

            return (trackOpt.Value as CustomTrack)?.folderPath;
        }

        /// <summary>
        /// Is <c>candidate</c> contained within <c>other</c>?
        /// </summary>
        /// <remarks>
        /// <para>
        /// Yoinked from https://stackoverflow.com/a/23354773
        /// </para>
        /// <para>
        /// May raise DirectoryInfo construction exceptions - see
        /// https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.-ctor?view=net-9.0 and
        /// https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.parent?view=net-9.0
        /// </para>
        /// </remarks>
        private static bool IsSubDirectoryOf(string candidate, string other)
        {
            var candidateInfo = new DirectoryInfo(candidate);
            var otherInfo = new DirectoryInfo(other);

            while (candidateInfo.Parent != null)
            {
                if (candidateInfo.Parent.FullName == otherInfo.FullName)
                {
                    return true;
                }
                else
                {
                    candidateInfo = candidateInfo.Parent;
                }
            }

            return false;
        }

        private void DeleteChart()
        {
            var dir = GetLocalChartDirectory();
            if (dir == null)
            {
                TootTallyNotifManager.DisplayNotif("Can only delete charts loaded by TrombLoader");
                return;
            }

            try
            {
                // Sanity check, we *do not* want to delete anything outside the CustomSongs dir
                var customSongsDir = Path.Combine(Paths.BepInExRootPath, "CustomSongs");
                if (!IsSubDirectoryOf(dir, customSongsDir))
                {
                    TootTallyNotifManager.DisplayNotif("Refusing to delete chart outside of the CustomSongs folder");
                    return;
                }

                try
                {
                    Directory.Delete(dir, recursive: true);
                }
                catch (IOException e)
                {
                    Plugin.LogError(e.ToString());
                    TootTallyNotifManager.DisplayNotif("IO error deleting chart");
                    return;
                }
                catch (UnauthorizedAccessException e)
                {
                    Plugin.LogError(e.ToString());
                    TootTallyNotifManager.DisplayNotif("Insufficient permissions while deleting chart");
                    return;
                }

                IsOwned = false;
                _songRow.WithIsDeletable(false);

                // Reload tracks when exiting this page
                ((SongDownloadPage)_page).MarkTrackDeleted(_song.track_ref);

                UpdateUiDownloadState();
                TryRequestFileData(); // If we don't know the file info (eg: filesize), request it now

                TootTallyNotifManager.DisplayNotif("Chart deleted");
            }
            catch (Exception e)
            {
                Plugin.LogError(e.ToString());
                TootTallyNotifManager.DisplayNotif("Failed to delete chart (check logs!)");
            }
        }

        private bool IsTrackDeletable(string trackRef)
        {
            var page = (SongDownloadPage)_page;
            return FSharpOption<TromboneTrack>.get_IsSome(TrackLookup.tryLookup(trackRef)) && !page.WasTrackDeleted(trackRef);
        }

        private bool HasTrackDownloaded(string trackRef)
        {
            var page = (SongDownloadPage)_page;
            return (FSharpOption<TromboneTrack>.get_IsSome(TrackLookup.tryLookup(trackRef)) && !page.WasTrackDeleted(trackRef))
                   || page.IsAlreadyDownloaded(trackRef);
        }

        /// <summary>
        /// Set <c>_fileData</c>, updating the UI along the way
        /// </summary>
        private void SetFileData(FileDataState newState)
        {
            _fileData = newState;
            UpdateUiDownloadState();
        }

        private void UpdateUiDownloadState()
        {
            if (HasTrackDownloaded(_song.track_ref))
            {
                _songRow.WithDownloadState(new DownloadState.Owned());
            }
            else
            {
                switch (_fileData)
                {
                    case FileDataState.ErrorFetchingData:
                        Debug.LogError($"NAH DATA");
                        _songRow.WithDownloadState(new DownloadState.DownloadUnavailable());
                        break;

                    case FileDataState.HasData hasData:
                        // "extension" isn't actually the extension here, it's the last part of the mime type >.<
                        switch (hasData.FileData.extension)
                        {
                            case "zip":
                            case "x-zip":
                            case "zip-compressed":
                            case "x-zip-compressed":
                                _songRow
                                    .WithFileSize(hasData.FileData.size)
                                    .WithDownloadState(new DownloadState.DownloadAvailable());
                                break;

                            default:
                                _songRow.WithDownloadState(new DownloadState.DownloadUnavailable());
                                break;
                        }
                        break;

                    case FileDataState.Unknown:
                    case FileDataState.WaitingOnRequest:
                        _songRow.WithDownloadState(new DownloadState.Waiting());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(_fileData));
                }
            }

            var page = (SongDownloadPage)_page;
            page.UpdateDownloadAllButton();
        }

        private void TryRequestFileData()
        {
            if (_fileData is not FileDataState.Unknown)
            {
                // Already requested file data, don't try again
                return;
            }

            var link = FileHelper.GetDownloadLinkFromSongData(_song);
            if (link != null)
            {
                _songRow.WithDownloadState(new DownloadState.Waiting());
                SetFileData(
                    new FileDataState.WaitingOnRequest(
                        Plugin.Instance.StartCoroutine(TootTallyAPIService.GetFileSize(link, OnFileDataFetched))
                    )
                );
            }
            else
            {
                Plugin.LogWarning($"{_song.track_ref} cannot be downloaded: no download link found");
                SetFileData(new FileDataState.ErrorFetchingData());
            }
        }

        private void OnFileDataFetched(FileHelper.FileData? fileData)
        {
            if (fileData != null)
            {
                if (!fileData.extension.Contains("zip"))
                {
                    Plugin.LogWarning($"{_song.track_ref} cannot be downloaded: File is not zip: {fileData.extension}");
                    SetFileData(new FileDataState.ErrorFetchingData());
                }
                else
                {
                    SetFileData(new FileDataState.HasData(fileData));
                }
            }
            else
            {
                Plugin.LogWarning($"{_song.track_ref} cannot be downloaded: got null FileData");
            }
        }
    }
}