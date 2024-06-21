#nullable enable

using TootTallyCore.Utils.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

public class DownloadButton
{
    private readonly GameObject _gameObject;
    private RectTransform Transform => (RectTransform)_gameObject.transform;

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private DownloadButton(GameObject gameObject)
    {
        _gameObject = gameObject;
    }

    internal static DownloadButton Create()
    {
        // ======== GameObject hierarchy ========
        // Body
        // |
        // +- Fill
        // +- Border
        // +- Icon
        // +- TODO: Add filesize, loading spinner, other icons?
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
            new Vector2(0.5f, 0.5f),
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
            new Vector2(0.5f, 0.5f),
            100f,
            1,
            SpriteMeshType.FullRect,
            new Vector4(4f, 4f, 4f, 4f),
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
        iconTf.anchorMin = new Vector2(0.5f, 0.5f);
        iconTf.anchorMax = new Vector2(0.5f, 0.5f);
        iconTf.offsetMin = new Vector2(-24f, -24f);
        iconTf.offsetMax = new Vector2(24f, 24f);

        iconGo.GetComponent<Image>().sprite = AssetManager.GetSprite("Download64.png");

        return new DownloadButton(bodyGo);
    }

    internal DownloadButton WithParent(Transform parent)
    {
        Transform.SetParent(parent, false);
        return this;
    }
}
