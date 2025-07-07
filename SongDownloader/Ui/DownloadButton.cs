#nullable enable

using System;
using TMPro;
using TootTallyCore.Graphics;
using TootTallyCore.Utils.Assets;
using TootTallyCore.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

public class DownloadButton
{
    private readonly GameObject _gameObject;
    private RectTransform Transform => (RectTransform)_gameObject.transform;

    private readonly Button _button;
    private readonly Image _fillImage;
    private readonly Image _downloadIconImage;
    private readonly TMP_Text _filesizeText;
    private readonly Image _altIconImage;
    private readonly LoadingIcon _loadingIcon;
    private readonly ProgressPieGraphic _progressPie;

    private DownloadState _downloadState = new DownloadState.Waiting();

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private DownloadButton(
        GameObject gameObject,
        Button button,
        Image fillImage,
        Image downloadIconImage,
        TMP_Text filesizeText,
        Image altIconImage,
        LoadingIcon loadingIcon,
        ProgressPieGraphic progressPie
    )
    {
        _gameObject = gameObject;
        _button = button;
        _fillImage = fillImage;
        _downloadIconImage = downloadIconImage;
        _filesizeText = filesizeText;
        _altIconImage = altIconImage;
        _loadingIcon = loadingIcon;
        _progressPie = progressPie;
    }

    internal static DownloadButton Create()
    {
        // ======== GameObject hierarchy ========
        // Body
        // |
        // +- Fill
        // +- Border
        // +- DownloadIcon
        // +- FilesizeText
        // +- AltIcon
        // +- LoadingIcon
        // +- ProgressPie
        // ======================================

        // Create body ////////////////////////////////////////////////////////////////////////////////////////////////
        var bodyGo = new GameObject(
            "DownloadButton",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster),
            typeof(LayoutElement),
            typeof(Button)
        );
        var bodyTf = bodyGo.transform;

        var bodyLayoutElem = bodyGo.GetComponent<LayoutElement>();
        bodyLayoutElem.preferredWidth = 64f;

        var button = bodyGo.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            Debug.Log("OHAI!");
        });

        // Create panel fill ///////////////////////////////////////////////////////////////////////////////////////////
        var fillGo = new GameObject(
            "PanelFill",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        var fillTf = (RectTransform)fillGo.transform;
        fillTf.SetParent(bodyTf, false);
        fillTf.anchorMin = Vector2.zero;
        fillTf.anchorMax = Vector2.one;
        fillTf.offsetMin = Vector2.zero;
        fillTf.offsetMax = Vector2.zero;

        var fillImage = fillGo.GetComponent<Image>();
        var fillTexture = AssetManager.GetTexture("RoundedBoxRightFill.png");
        fillImage.sprite = Sprite.Create(
            fillTexture,
            new Rect(0f, 0f, fillTexture.width, fillTexture.height),
            Vector2.one * .5f,
            100f,
            1,
            SpriteMeshType.FullRect,
            new Vector4(4f, 4f, 4f, 4f),
            false
        );
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(0.910f, 0.212f, 0.333f); // TODO: Use theme colors

        button.targetGraphic = fillImage;

        // Create panel border /////////////////////////////////////////////////////////////////////////////////////////
        var borderGo = new GameObject(
            "PanelBorder",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        var borderTf = (RectTransform)borderGo.transform;
        borderTf.SetParent(bodyTf, false);
        borderTf.anchorMin = Vector2.zero;
        borderTf.anchorMax = Vector2.one;
        borderTf.offsetMin = Vector2.zero;
        borderTf.offsetMax = Vector2.zero;

        var borderImage = borderGo.GetComponent<Image>();
        var borderTexture = AssetManager.GetTexture("RoundedBoxRightBorder.png");
        borderImage.sprite = Sprite.Create(
            borderTexture,
            new Rect(0f, 0f, borderTexture.width, borderTexture.height),
            Vector2.one * .5f,
            100f,
            1,
            SpriteMeshType.FullRect,
            new Vector4(4f, 4f, 4f, 4f),
            false
        );
        borderImage.type = Image.Type.Sliced;
        borderImage.color = new Color(0.910f, 0.212f, 0.333f); // TODO: Use theme colors

        // Create main download icon ///////////////////////////////////////////////////////////////////////////////////
        // Just an icon that isn't the main download icon
        var downloadIconGo = new GameObject(
            "DownloadIcon",
            typeof(RectTransform),
            typeof(Image)
        );
        downloadIconGo.SetActive(false);

        var downloadIconImage = downloadIconGo.GetComponent<Image>();
        downloadIconImage.sprite = AssetManager.GetSprite("Download64.png");

        var anchor = Vector2.one * .5f;
        // Center-ish the icon
        // (the icon is placed a bit above the center to make room for the filesize text)
        var downloadIconTf = (RectTransform)downloadIconGo.transform;
        downloadIconTf.SetParent(bodyTf, false);
        downloadIconTf.anchorMin = downloadIconTf.anchorMax = anchor;
        downloadIconTf.offsetMin = new Vector2(-18f, -18f + 8f);
        downloadIconTf.offsetMax = new Vector2(18f, 18f + 8f);

        // Create filesize text ////////////////////////////////////////////////////////////////////////////////////////
        var filesizeText = GameObjectFactory.CreateSingleText(bodyTf, "FilesizeText", "12.3 MB");
        filesizeText.gameObject.SetActive(false);

        var filesizeTf = filesizeText.rectTransform!;
        filesizeTf.anchorMin = Vector2.zero;
        filesizeTf.anchorMax = new Vector2(1f, 0f);
        filesizeTf.offsetMin = Vector2.one * 4f;
        filesizeTf.offsetMax = new Vector2(-4f, 24f);

        filesizeText.fontSize = 14f;
        filesizeText.alignment = TextAlignmentOptions.Bottom;
        filesizeText.overflowMode = TextOverflowModes.Masking;
        filesizeText.enableWordWrapping = false;

        // Create alt icon /////////////////////////////////////////////////////////////////////////////////////////////
        // Just an icon that isn't the main download icon
        var altIconGo = new GameObject(
            "AltIcon",
            typeof(RectTransform),
            typeof(Image)
        );
        altIconGo.SetActive(false);

        var offset = Vector2.one * 24f;

        // Center the icon
        var altIconTf = (RectTransform)altIconGo.transform;
        altIconTf.SetParent(bodyTf, false);
        altIconTf.anchorMin = altIconTf.anchorMax = anchor;
        altIconTf.offsetMin = -offset;
        altIconTf.offsetMax = offset;

        // Create loading icon /////////////////////////////////////////////////////////////////////////////////////////
        var loadingIcon = GameObjectFactory.CreateLoadingIcon(
            bodyTf,
            Vector2.zero,
            Vector2.one * 48f,
            AssetManager.GetSprite("IconMono.png"),
            true,
            "LoadingIcon"
        );
        var loadingIconTf = (RectTransform)loadingIcon.iconHolder.transform;
        loadingIconTf.anchorMin = loadingIconTf.anchorMax = anchor;
        loadingIconTf.offsetMin = -offset;
        loadingIconTf.offsetMax = offset;

        // Create progress pie /////////////////////////////////////////////////////////////////////////////////////////
        // Used when the chart is downloading
        var progressPieGo = new GameObject(
            "ProgressPie",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(ProgressPieGraphic)
        );
        progressPieGo.SetActive(false);

        // Center the pie
        var progressPieTf = (RectTransform)progressPieGo.transform;
        progressPieTf.SetParent(bodyTf, false);
        progressPieTf.anchorMin = progressPieTf.anchorMax = anchor;
        progressPieTf.offsetMin = -offset;
        progressPieTf.offsetMax = offset;

        return new DownloadButton(
            bodyGo,
            button,
            fillImage,
            downloadIconImage,
            filesizeText,
            altIconGo.GetComponent<Image>(),
            loadingIcon,
            progressPieGo.GetComponent<ProgressPieGraphic>()
        );
    }

    internal DownloadButton WithParent(Transform parent)
    {
        Transform.SetParent(parent, false);
        return this;
    }

    internal DownloadButton WithFilesize(long numBytes)
    {
        _filesizeText.text = FileHelper.SizeSuffix(numBytes, 1);
        return this;
    }

    internal DownloadButton WithDownloadState(DownloadState state)
    {
        _downloadState = state;

        switch (state)
        {
            case DownloadState.DownloadAvailable:
                _downloadIconImage.gameObject.SetActive(true);
                _filesizeText.gameObject.SetActive(true);
                _altIconImage.gameObject.SetActive(false);
                _loadingIcon.iconHolder.SetActive(false);
                _loadingIcon.StopRecursiveAnimation(true);
                _progressPie.gameObject.SetActive(false);
                _fillImage.color = new Color(0.910f, 0.212f, 0.333f); // TODO: Use theme colors
                break;

            case DownloadState.DownloadUnavailable:
                _downloadIconImage.gameObject.SetActive(false);
                _filesizeText.gameObject.SetActive(false);
                _altIconImage.gameObject.SetActive(true);
                _loadingIcon.iconHolder.SetActive(false);
                _loadingIcon.StopRecursiveAnimation(true);
                _progressPie.gameObject.SetActive(false);
                _fillImage.color = new Color(0.102f, 0.102f, 0.102f); // TODO: Use theme colors

                _altIconImage.sprite = AssetManager.GetSprite("PageBroken64.png");
                break;

            case DownloadState.Downloading counter:
                _downloadIconImage.gameObject.SetActive(false);
                _filesizeText.gameObject.SetActive(false);
                _altIconImage.gameObject.SetActive(false);
                _loadingIcon.iconHolder.SetActive(false);
                _loadingIcon.StopRecursiveAnimation(true);
                _progressPie.gameObject.SetActive(true);
                counter.Progress.AddCounter(_progressPie);
                _fillImage.color = new Color(0.102f, 0.102f, 0.102f); // TODO: Use theme colors
                break;

            case DownloadState.Owned:
                _downloadIconImage.gameObject.SetActive(false);
                _filesizeText.gameObject.SetActive(false);
                _altIconImage.gameObject.SetActive(true);
                _loadingIcon.iconHolder.SetActive(false);
                _loadingIcon.StopRecursiveAnimation(true);
                _progressPie.gameObject.SetActive(false);
                _fillImage.color = new Color(0.102f, 0.102f, 0.102f); // TODO: Use theme colors

                _altIconImage.sprite = AssetManager.GetSprite("PageTick64.png");
                break;

            case DownloadState.Waiting:
                _downloadIconImage.gameObject.SetActive(false);
                _filesizeText.gameObject.SetActive(false);
                _altIconImage.gameObject.SetActive(false);
                _loadingIcon.iconHolder.SetActive(true);
                _loadingIcon.StartRecursiveAnimation();
                _progressPie.gameObject.SetActive(false);
                _fillImage.color = new Color(0.102f, 0.102f, 0.102f); // TODO: Use theme colors
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state));
        }

        return this;
    }

    internal void OnDownload(Action callback)
    {
        _button.onClick.AddListener(() =>
        {
            if (_downloadState is DownloadState.DownloadAvailable)
            {
                callback();
            }
        });
    }
}
