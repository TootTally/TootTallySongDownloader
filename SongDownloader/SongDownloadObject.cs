#nullable enable

using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Core;
using System.IO;
using TMPro;
using TootTallyCore.APIServices;
using TootTallyCore.Graphics;
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
        private SongRow? _songRow;
        private SongDataFromDB? _song;
        private GameObject? _downloadButton;
        private ProgressBar? _progressBar;
        private TMP_Text? _fileSizeText;
        private TMP_Text? _durationText;
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
                .WithDifficulty(song.difficulty);

            // TODO ////////////////////////////////////////////////////////////////////////////////////////////////////

            // var charterText = GameObjectFactory.CreateSingleText(mainBodyTf, "Charter", song.charter != null ? $"Mapped by {song.charter}" : "Unknown");
            // _durationText = GameObjectFactory.CreateSingleText(mainBodyTf, "Duration", stringTime);
            // _fileSizeText = GameObjectFactory.CreateSingleText(mainBodyTf, "FileSize", "");
            // _fileSizeText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 128);
            // _fileSizeText.gameObject.SetActive(false);

            //fuck that shit :skull:
            // songNameText.GetComponent<RectTransform>().sizeDelta = charterText.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 128);
            // _durationText.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 128);
            // songNameText.overflowMode = charterText.overflowMode = _durationText.overflowMode = TextOverflowModes.Ellipsis;

            // TODO TEMP
            // charterText.gameObject.SetActive(false);
            // _durationText.gameObject.SetActive(false);

            //lol
            if (FSharpOption<TromboneTrack>.get_IsNone(TrackLookup.tryLookup(song.track_ref)) && !(_page as SongDownloadPage).IsAlreadyDownloaded(song.track_ref))
            {
                string? link = FileHelper.GetDownloadLinkFromSongData(song);

                if (link != null)
                {
                    _fileSizeCoroutine = Plugin.Instance.StartCoroutine(TootTallyAPIService.GetFileSize(link, fileData =>
                    {
                        if (fileData != null)
                        {
                            DisplaySizeFileText(fileData.size);
                            if (!fileData.extension.Contains("zip"))
                                DisplayNotAvailableText($"File is not zip: {fileData.extension}.", 4);
                            else
                            {
                                _fileSizeCoroutine = null;
                                // TODO
                                // isDownloadAvailable = true;
                                // _downloadButton = GameObjectFactory.CreateCustomButton(downloadBodyTf, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("Download64.png"), "DownloadButton", DownloadChart).gameObject;
                                // _downloadButton.transform.SetSiblingIndex(4);
                                // _progressBar = GameObjectFactory.CreateProgressBar(_songRow.transform.Find("LatencyFG"), Vector2.zero, new Vector2(900, 20), false, "ProgressBar");
                                // ((SongDownloadPage)_page).UpdateDownloadAllButton();
                            }
                        }
                        else
                        {
                            DisplayNotAvailableText($"Couldn't access file at {link}", 3);
                        }
                        
                    }));
                }
                else
                    DisplayNotAvailableText("No download link found.");
            }
            else
                DisplayOwnedText();

            //GameObjectFactory.CreateCustomButton(_songRow.transform, Vector2.zero, new Vector2(64, 64), AssetManager.GetSprite("global64.png"), "OpenWebButton", () => Application.OpenURL($"https://toottally.com/song/{song.id}/"));
        }

        public void DisplaySizeFileText(long size)
        {
            // var stringSize = FileHelper.SizeSuffix(size, 2);
            // _fileSizeText.text = stringSize;
            // _fileSizeText.gameObject.SetActive(true);
            // _durationText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 128);

            // TODO
        }

        public void DisplayNotAvailableText(string error, int siblingIndex = -1)
        {
            Plugin.LogWarning($"{_song.track_ref} cannot be downloaded: {error}");
            var notAvailableText = GameObjectFactory.CreateSingleText(_songRow!.GameObject.transform, "N/A", "N/A");
            notAvailableText.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
            notAvailableText.overflowMode = TextOverflowModes.Overflow;
            notAvailableText.enableWordWrapping = false;
            if (siblingIndex != -1)
                notAvailableText.transform.SetSiblingIndex(siblingIndex);
        }

        public void DisplayOwnedText()
        {
            isOwned = true;
            var ownedText = GameObjectFactory.CreateSingleText(_songRow!.GameObject.transform, "Owned", "Owned");
            ownedText.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
            ownedText.overflowMode = TMPro.TextOverflowModes.Overflow;
            ownedText.enableWordWrapping = false;
        }

        public void SetActive(bool active) => _songRow!.GameObject.SetActive(active);

        public override void Dispose()
        {
            Object.DestroyImmediate(_songRow!.GameObject);
            if (_fileSizeCoroutine != null)
                Plugin.Instance.StopCoroutine(_fileSizeCoroutine);
        }

        public void DownloadChart()
        {
            isDownloadAvailable = false;
            _downloadButton.SetActive(false);
            _fileSizeText.gameObject.SetActive(false);
            string link = _song.mirror ?? _song.download;
            Plugin.Instance.StartCoroutine(TootTallyAPIService.DownloadZipFromServer(link, _progressBar, data =>
            {
                if (data != null)
                {
                    string downloadDir = Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location)!, "Downloads/");
                    string fileName = $"{_song.id}.zip";
                    if (!Directory.Exists(downloadDir))
                        Directory.CreateDirectory(downloadDir);
                    FileHelper.WriteBytesToFile(downloadDir, fileName, data);

                    string source = Path.Combine(downloadDir, fileName);
                    string destination = Path.Combine(Paths.BepInExRootPath, "CustomSongs/");
                    FileHelper.ExtractZipToDirectory(source, destination);

                    FileHelper.DeleteFile(downloadDir, fileName);

                    var t4 = GameObjectFactory.CreateSingleText(_songRow!.GameObject.transform, "Owned", "Owned");
                    isOwned = true;
                    t4.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 128);
                    t4.overflowMode = TMPro.TextOverflowModes.Overflow;
                    t4.enableWordWrapping = false;
                    t4.transform.SetSiblingIndex(3);
                    _durationText.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 128);
                    var page = _page as SongDownloadPage;
                    page.AddTrackRefToDownloadedSong(_song.track_ref);
                    SetActive(!page.ShowNotOwnedOnly);
                }
                else
                {
                    TootTallyNotifManager.DisplayNotif("Download failed.");
                    _downloadButton.SetActive(true);
                }

            }));
        }

    }
}
