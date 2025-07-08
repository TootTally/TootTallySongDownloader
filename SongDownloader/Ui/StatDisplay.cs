#nullable enable

using TMPro;
using TootTallyCore.Graphics;
using TootTallyCore.Utils.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

/// <summary>
/// An icon and some text, next to each other
/// </summary>
public class StatDisplay
{
    private readonly GameObject _gameObject;
    private RectTransform Transform => (RectTransform)_gameObject.transform;

    private readonly Image _image;
    private readonly TMP_Text _text;

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private StatDisplay(GameObject gameObject, Image image, TMP_Text text)
    {
        _gameObject = gameObject;
        _image = image;
        _text = text;
    }

    internal static StatDisplay Create()
    {
        // ======== GameObject hierarchy ========
        // Body
        // |
        // +- Icon
        // +- Text
        // ======================================

        // Create body ////////////////////////////////////////////////////////////////////////////////////////////////
        var bodyGo = new GameObject(
            "StatDisplay",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );
        var bodyTf = bodyGo.transform;

        var layout = bodyGo.GetComponent<HorizontalLayoutGroup>()!;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 4f;

        // TODO: Add tooltip explaining what the thing is?

        // Create icon /////////////////////////////////////////////////////////////////////////////////////////////////
        var icon = new GameObject(
            "Icon",
            typeof(RectTransform),
            typeof(Image)
        );

        var iconTf = (RectTransform)icon.transform;
        iconTf.SetParent(bodyTf, false);

        var text = GameObjectFactory.CreateSingleText(bodyTf, "Text", "");
        text.fontSize = 18f;
        text.enableWordWrapping = false;
        text.autoSizeTextContainer = true;

        return new StatDisplay(bodyGo, icon.GetComponent<Image>(), text);
    }

    internal StatDisplay WithParent(Transform parent)
    {
        Transform.SetParent(parent, false);
        return this;
    }

    internal StatDisplay WithIcon(string assetKey)
    {
        _image.sprite = AssetManager.GetSprite(assetKey);
        ((RectTransform)_image.transform).sizeDelta = Vector2.one * 20f; // TODO: Not here?
        return this;
    }

    internal StatDisplay WithText(string text)
    {
        _text.text = text;
        return this;
    }
}
