using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTallyCore.APIServices;
using TootTallyCore.Graphics;
using TootTallyCore.Utils.Assets;
using TootTallyCore.Utils.Helpers;
using TootTallyCore.Utils.TootTallyNotifs;
using TootTallySettings;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.SongDownloader
{
    public class ChartPackDownloadPage : TootTallySettingPage
    {
        private const string DEFAULT_INPUT_TEXT = "SearchHere";
        private TMP_InputField _inputField;
        private GameObject _searchButton;
        private GameObject _nextButton, _prevButton;
        private LoadingIcon _loadingIcon;
        private List<string> _newDownloadedTrackRefs;
        public ChartPackDownloadPage() : base("Chart Packs", "Chart Packs", 20f, new Color(0, 0, 0, 0.1f), GetButtonColors)
        {
        }

        private static ColorBlock GetButtonColors => new ColorBlock()
        {
            normalColor = new Color(.20f, .20f, 1),
            highlightedColor = new Color(.15f, .15f, .65f),
            pressedColor = new Color(.5f, .5f, .75f),
            selectedColor = new Color(.20f, .20f, 1),
            fadeDuration = .08f,
            colorMultiplier = 1
        };

        public override void Initialize()
        {
            base.Initialize();

            _inputField = TootTallySettingObjectFactory.CreateInputField(_fullPanel.transform, $"{name}InputField", DEFAULT_OBJECT_SIZE, DEFAULT_FONTSIZE, DEFAULT_INPUT_TEXT, false);
            _inputField.onSubmit.AddListener(value => Search(_inputField.text));
            _inputField.GetComponent<RectTransform>().anchorMin = _inputField.GetComponent<RectTransform>().anchorMax = new Vector2(.72f, .7f);

            _loadingIcon = GameObjectFactory.CreateLoadingIcon(_fullPanel.transform, new Vector2(-300, -75), new Vector2(128, 128), AssetManager.GetSprite("icon.png"), false, "SongSearchLoadingSwirly");

            _searchButton = GameObjectFactory.CreateCustomButton(_fullPanel.transform, new Vector2(-375, -175), DEFAULT_OBJECT_SIZE, "Search", $"{name}SearchButton", () => Search(_inputField.text)).gameObject;

            _backButton.button.onClick.AddListener(() =>
            {
                if (_newDownloadedTrackRefs.Count > 0)
                {
                    TootTallyNotifManager.DisplayNotif("New tracks detected, Reloading songs...\nLagging is normal.");
                    _newDownloadedTrackRefs.Clear();
                    TootTallyCore.Plugin.Instance.reloadManager.ReloadAll(new ProgressCallbacks()
                    {
                        onComplete = () =>
                        {
                            TootTallyNotifManager.DisplayNotif("Reloading complete!");
                        },
                        onError = err =>
                        {
                            TootTallyNotifManager.DisplayNotif($"Reloading failed! {err.Message}");
                        }
                    });
                }
            });
        }

        public override void OnShow()
        {
            _loadingIcon.StartRecursiveAnimation();
        }

        public override void OnHide()
        {
            _loadingIcon.StopRecursiveAnimation(true);
        }

        private void Search(string input, bool searchWithFilter = true)
        {
            if (input == DEFAULT_INPUT_TEXT)
                input = "";
            // _downloadObjectList.Clear();
            // _downloadAllButton.SetActive(false);
            RemoveAllObjects();
            _searchButton.SetActive(false);
            _loadingIcon.Show();
            if (_nextButton != null)
            {
                GameObject.DestroyImmediate(_nextButton);
                _nextButton = null;
            }
            if (_prevButton != null)
            {
                GameObject.DestroyImmediate(_prevButton);
                _prevButton = null;
            }

            // _trackRefList.Clear();
            // if (searchWithFilter)
            //     Plugin.Instance.StartCoroutine(TootTallyAPIService.SearchSongWithFilters(input, _toggleRated.isOn, _toggleUnrated.isOn, OnSearchInfoRecieved));
            // else
            //     Plugin.Instance.StartCoroutine(TootTallyAPIService.SearchSongByURL(input, OnSearchInfoRecieved));

        }
    }
}
