#nullable enable

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

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private MoreInfoButton(GameObject gameObject)
    {
        _gameObject = gameObject;
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

        return new MoreInfoButton(bodyGo);
    }

    internal MoreInfoButton WithParent(Transform parent)
    {
        Transform.SetParent(parent, false);
        return this;
    }
}
