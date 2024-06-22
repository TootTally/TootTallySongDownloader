#nullable enable

using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Core;
using System.IO;
using TootTallyCore.APIServices;
using TootTallyCore.Graphics.ProgressCounter;
using TootTallyCore.Utils.Helpers;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallySettings;
using TootTallySongDownloader.Ui;
using UnityEngine;
using static TootTallyCore.APIServices.SerializableClass;

namespace TootTallySongDownloader
{
    internal class SongDownloadObject : BaseTootTallySettingObject
    {
        private readonly SongRow _songRow;
        private readonly SongDataFromDB _song;
        public bool isDownloadAvailable, isOwned;
        private Coroutine? _fileSizeCoroutine;

        public SongDownloadObject(Transform canvasTransform, SongDataFromDB song, TootTallySettingPage page) : base($"Song{song.track_ref}", page)
        {
            _song = song;

            _songRow = SongRow.Create()
                .WithParent(canvasTransform)
                .WithSongName(song.name)
                .WithArtist(song.author)
                .WithDurationSeconds(song.song_length)
                .WithDifficulty(song.difficulty)
                .WithCharter(song.charter)
                .OnDownload(DownloadChart);

            //lol
            if (FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(song.track_ref)) && !((SongDownloadPage)_page).IsAlreadyDownloaded(song.track_ref))
            {
                // User does not have the chart downloaded, query some info from the TootTally API
                var link = FileHelper.GetDownloadLinkFromSongData(song);
                if (link != null)
                {
                    _songRow.WithDownloadState(new DownloadState.Waiting());
                    _fileSizeCoroutine = Plugin.Instance.StartCoroutine(TootTallyAPIService.GetFileSize(link, fileData =>
                    {
                        if (fileData != null)
                        {
                            if (!fileData.extension.Contains("zip"))
                            {
                                DisplaySongNotAvailable($"File is not zip: {fileData.extension}.");
                            }
                            else
                            {
                                _fileSizeCoroutine = null;
                                // TODO
                                // isDownloadAvailable = true;
                                // _downloadButton = GameObjectFactory.CreateCustomButton(downloadBodyTf, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("Download64.png"), "DownloadButton", DownloadChart).gameObject;
                                // _downloadButton.transform.SetSiblingIndex(4);
                                // _progressBar = GameObjectFactory.CreateProgressBar(_songRow.transform.Find("LatencyFG"), Vector2.zero, new Vector2(900, 20), false, "ProgressBar");
                                // ((SongDownloadPage)_page).UpdateDownloadAllButton();

                                _songRow
                                    .WithFileSize(fileData.size)
                                    .WithDownloadState(new DownloadState.DownloadAvailable());
                            }
                        }
                        else
                        {
                            DisplaySongNotAvailable($"Couldn't access file at {link}");
                        }
                    }));
                }
                else
                {
                    // TODO
                    DisplaySongNotAvailable("No download link found.");
                }
            }
            else
            {
                // User already has the song
                _songRow.WithDownloadState(new DownloadState.Owned());
            }

            //GameObjectFactory.CreateCustomButton(_songRow.transform, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("global64.png"), "OpenWebButton", () => Application.OpenURL($"https://toottally.com/song/{song.id}/"));
        }

        public void DisplaySongNotAvailable(string error)
        {
            Plugin.LogWarning($"{_song.track_ref} cannot be downloaded: {error}");
            _songRow.WithDownloadState(new DownloadState.DownloadUnavailable());
        }

        public void SetActive(bool active) => _songRow.GameObject.SetActive(active);

        public override void Dispose()
        {
            _songRow.Dispose();
            if (_fileSizeCoroutine != null)
                Plugin.Instance.StopCoroutine(_fileSizeCoroutine);
        }

        public void DownloadChart()
        {
            // isDownloadAvailable = false;

            var progress = new ProgressCounter();
            _songRow.WithDownloadState(new DownloadState.Downloading { Progress = progress });

            var link = _song.mirror ?? _song.download;
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadZipFromServer(
                link,
                progress,
                data =>
                {
                    if (data != null)
                    {
                        var downloadDir = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location)!, "Downloads/");
                        var fileName = $"{_song.id}.zip";
                        if (!Directory.Exists(downloadDir))
                            Directory.CreateDirectory(downloadDir);
                        FileHelper.WriteBytesToFile(downloadDir, fileName, data);

                        var source = Path.Combine(downloadDir, fileName);
                        var destination = Path.Combine(Paths.BepInExRootPath, "CustomSongs/");
                        FileHelper.ExtractZipToDirectory(source, destination);

                        FileHelper.DeleteFile(downloadDir, fileName);

                        isOwned = true;

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
    }
}
