#nullable enable

using System;
using System.Collections.Generic;
using TootTallyCore.Utils.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

/// <summary>
/// The ... button
/// </summary>
public class MoreInfoButton
{
    private readonly GameObject _gameObject;
    private RectTransform Transform => (RectTransform)_gameObject.transform;

    private int _songId;
    private bool _isDeletable = false;
    private DownloadState _downloadState = new DownloadState.Waiting();

    private readonly List<Action> _onDownloadFromTootTally;
    private readonly List<Action> _onDownloadFromAlternative;
    private readonly List<Action> _onDelete;

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private MoreInfoButton(GameObject gameObject)
    {
        _gameObject = gameObject;
        _onDownloadFromTootTally = new List<Action>();
        _onDownloadFromAlternative = new List<Action>();
        _onDelete = new List<Action>();
    }

    internal static MoreInfoButton Create()
    {
        // ======== GameObject hierarchy ========
        // Body
        // |
        // +- Fill
        // +- Border
        // +- Icon
        // ======================================

        // Create body ////////////////////////////////////////////////////////////////////////////////////////////////
        var bodyGo = new GameObject(
            "MoreInfoButton",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster),
            typeof(LayoutElement),
            typeof(Button)
        );
        var bodyTf = bodyGo.transform;

        var bodyLayoutElem = bodyGo.GetComponent<LayoutElement>();
        bodyLayoutElem.preferredWidth = 24f;

        var button = bodyGo.GetComponent<Button>();

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
        var fillTexture = AssetManager.GetTexture("HBoxMidFill.png");
        fillImage.sprite = Sprite.Create(
            fillTexture,
            new Rect(0f, 0f, fillTexture.width, fillTexture.height),
            Vector2.one * .5f,
            100f,
            1,
            SpriteMeshType.FullRect,
            Vector4.one * 4f,
            false
        );
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(0.557f, 0.129f, 0.204f); // TODO: Use theme colors

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
        var borderTexture = AssetManager.GetTexture("HBoxMidBorder.png");
        borderImage.sprite = Sprite.Create(
            borderTexture,
            new Rect(0f, 0f, borderTexture.width, borderTexture.height),
            Vector2.one * .5f,
            100f,
            1,
            SpriteMeshType.FullRect,
            Vector4.one * 4f,
            false
        );
        borderImage.type = Image.Type.Sliced;
        borderImage.color = new Color(0.910f, 0.212f, 0.333f); // TODO: Use theme colors

        // Create icon /////////////////////////////////////////////////////////////////////////////////////////////////
        var iconGo = new GameObject(
            "Icon",
            typeof(RectTransform),
            typeof(Image)
        );

        // Center the icon
        var iconTf = (RectTransform)iconGo.transform;
        iconTf.SetParent(bodyTf, false);
        iconTf.anchorMin = Vector2.one * 0.5f;
        iconTf.anchorMax = Vector2.one * 0.5f;
        iconTf.offsetMin = new Vector2(-3f, -11.5f);
        iconTf.offsetMax = new Vector2(3f, 11.5f);

        iconGo.GetComponent<Image>().sprite = AssetManager.GetSprite("MoreInfoIcon.png");

        var ret = new MoreInfoButton(bodyGo);
        button.onClick.AddListener(() => ret.OpenMenu());

        return ret;
    }

    internal MoreInfoButton WithParent(Transform parent)
    {
        Transform.SetParent(parent, false);
        return this;
    }

    internal MoreInfoButton WithSongId(int songId)
    {
        _songId = songId;
        return this;
    }

    internal MoreInfoButton WithDownloadState(DownloadState downloadState)
    {
        _downloadState = downloadState;
        return this;
    }

    internal MoreInfoButton WithIsDeletable(bool isDeletable)
    {
        _isDeletable = isDeletable;
        return this;
    }

    internal MoreInfoButton OnDownloadFromTootTally(Action callback)
    {
        _onDownloadFromTootTally.Add(callback);
        return this;
    }

    internal MoreInfoButton OnDownloadFromAlternative(Action callback)
    {
        _onDownloadFromAlternative.Add(callback);
        return this;
    }

    internal MoreInfoButton OnDelete(Action callback)
    {
        _onDelete.Add(callback);
        return this;
    }

    private void OpenMenu()
    {
        var menu = MenuOverlay.Create()
            .WithItem(
                "View on TootTally.com",
                () => Application.OpenURL($"https://toottally.com/song/{_songId}/")
            );

        switch (_downloadState)
        {
            case DownloadState.DownloadAvailable:
                menu.WithItem(
                    "Download from TootTally",
                    () => _onDownloadFromTootTally.ForEach(action => action())
                );
                menu.WithItem(
                    "Download from alternate source",
                    () => _onDownloadFromAlternative.ForEach(action => action())
                );
                break;

            case DownloadState.DownloadUnavailable:
                menu.WithItem(
                    "Try download anyway from TootTally",
                    () => _onDownloadFromTootTally.ForEach(action => action())
                );
                menu.WithItem(
                    "Try download anyway from alternate source",
                    () => _onDownloadFromAlternative.ForEach(action => action())
                );
                break;

            case DownloadState.Owned:
                menu.WithItem(
                    "Redownload from TootTally",
                    () => _onDownloadFromTootTally.ForEach(action => action())
                );
                menu.WithItem(
                    "Redownload from alternate source",
                    () => _onDownloadFromAlternative.ForEach(action => action())
                );
                break;

            case DownloadState.Waiting:
            case DownloadState.Downloading:
                // Don't show download options
                break;
        }

        if (_isDeletable)
        {
            menu.WithItem(
                "Delete",
                () => _onDelete.ForEach(action => action())
            );
        }

        menu.ShowAtCursor();
    }
}