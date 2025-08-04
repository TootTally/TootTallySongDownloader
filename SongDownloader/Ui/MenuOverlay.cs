using TMPro;
using TootTallyCore.Graphics;
using TootTallyCore.Graphics.Animations;
using TootTallyCore.Utils.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

internal class MenuOverlay
{
    private readonly GameObject _canvasGameObject;
    private readonly GameObject _bodyGameObject;

    private bool _isHiding = false;

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private MenuOverlay(
        GameObject canvasGameObject,
        GameObject bodyGameObject
    )
    {
        _canvasGameObject = canvasGameObject;
        _bodyGameObject = bodyGameObject;
    }

    internal static MenuOverlay Create()
    {
        // Create a new canvas in the scene
        var canvasGo = new GameObject(
            "MenuOverlayCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup)
        );
        var canvasTf = canvasGo.transform;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 2;
        var canvasScaler = canvasGo.GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Fullscreen invisible button to close the menu ///////////////////////////////////////////////////////////////
        var closeButtonGo = new GameObject(
            "CloseButton",
            typeof(RectTransform),
            typeof(GraphicRaycaster),
            typeof(Button),
            typeof(Image)
        );

        var closeButtonTf = (RectTransform)closeButtonGo.transform;
        closeButtonTf.SetParent(canvasTf, false);
        closeButtonTf.anchorMin = Vector2.zero;
        closeButtonTf.anchorMax = Vector2.one;
        closeButtonTf.offsetMin = Vector2.zero;
        closeButtonTf.offsetMax = Vector2.zero;

        var closeButton = closeButtonGo.GetComponent<Button>();

        // Need an image, even if it is invisible, as a hit target
        var closeButtonImage = closeButtonGo.GetComponent<Image>();
        closeButtonImage.color = new Color(0f, 0f, 0f, 0f);

        // Menu body ///////////////////////////////////////////////////////////////////////////////////////////////////
        var mainBodyGo = new GameObject(
            "MenuBody",
            typeof(RectTransform),
            typeof(OverlayLayoutGroup),
            typeof(ContentSizeFitter)
        );

        var mainBodyTf = (RectTransform)mainBodyGo.transform;
        mainBodyTf.SetParent(canvasTf, false);

        var mainBodyCsf = mainBodyGo.GetComponent<ContentSizeFitter>();
        mainBodyCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        mainBodyCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Add main panel fill /////////////////////////////////////////////////////////////////////////////////////////
        var panelFill = new GameObject(
            "PanelFill",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        var panelFillTf = (RectTransform)panelFill.transform;
        panelFillTf.SetParent(mainBodyTf, false);
        panelFillTf.anchorMin = Vector2.zero;
        panelFillTf.anchorMax = Vector2.one;
        panelFillTf.offsetMin = Vector2.zero;
        panelFillTf.offsetMax = Vector2.zero;

        var panelFillImage = panelFill.GetComponent<Image>();
        var fillLeftTexture = AssetManager.GetTexture("RoundedBoxLeftFill.png");
        panelFillImage.sprite = Sprite.Create(
            fillLeftTexture,
            new Rect(0f, 0f, fillLeftTexture.width, fillLeftTexture.height),
            Vector2.one * .5f,
            100f,
            1,
            SpriteMeshType.FullRect,
            Vector4.one * 4f,
            false
        );
        panelFillImage.type = Image.Type.Sliced;
        panelFillImage.color = new Color(0.102f, 0.102f, 0.102f); // TODO: Use theme colors

        // Add main panel border ///////////////////////////////////////////////////////////////////////////////////////
        var panelBorderGo = new GameObject(
            "PanelBorder",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        var panelBorderTf = (RectTransform)panelBorderGo.transform;
        panelBorderTf.SetParent(mainBodyTf, false);
        panelBorderTf.anchorMin = Vector2.zero;
        panelBorderTf.anchorMax = Vector2.one;
        panelBorderTf.offsetMin = Vector2.zero;
        panelBorderTf.offsetMax = Vector2.zero;

        var panelBorderImage = panelBorderGo.GetComponent<Image>();
        var borderLeftTexture = AssetManager.GetTexture("RoundedBoxLeftBorder.png");
        panelBorderImage.sprite = Sprite.Create(
            borderLeftTexture,
            new Rect(0f, 0f, borderLeftTexture.width, borderLeftTexture.height),
            Vector2.one * .5f,
            100f,
            1,
            SpriteMeshType.FullRect,
            Vector4.one * 4f,
            false
        );
        panelBorderImage.type = Image.Type.Sliced;
        panelBorderImage.color = new Color(0.910f, 0.212f, 0.333f); // TODO: Use theme colors

        // Add list-of-items ///////////////////////////////////////////////////////////////////////////////////////////
        var itemListGo = new GameObject(
            "ItemList",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup)
        );

        var itemListTf = itemListGo.transform;
        itemListTf.SetParent(mainBodyTf, false);

        var itemListVlg = itemListGo.GetComponent<VerticalLayoutGroup>();
        itemListVlg.padding = new RectOffset(1, 1, 1, 1);

        var ret = new MenuOverlay(canvasGo, mainBodyGo);
        closeButton.onClick.AddListener(ret.Hide);

        return ret;
    }

    internal MenuOverlay WithItem(string text, UnityAction onClick)
    {
        var itemListTf = _bodyGameObject.transform.Find("ItemList");

        var buttonGo = new GameObject(
            "Item",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Button),
            typeof(Image),
            typeof(OverlayLayoutGroup)
        );

        var buttonTf = buttonGo.transform;
        buttonTf.SetParent(itemListTf, false);

        var buttonImage = buttonGo.GetComponent<Image>();

        var buttonButton = buttonGo.GetComponent<Button>();
        buttonButton.transition = Selectable.Transition.ColorTint;
        buttonButton.targetGraphic = buttonImage;
        buttonButton.colors = new ColorBlock
        {
            normalColor = new Color(0f, 0f, 0f, 0f),
            highlightedColor = new Color(0.910f, 0.212f, 0.333f), // TODO: Use theme colors
            pressedColor = new Color(0.557f, 0.129f, 0.204f), // TODO: Use theme colors
            colorMultiplier = 1f,
        };
        buttonButton.onClick.AddListener(() =>
        {
            onClick();
            Hide();
        });

        var buttonOlg = buttonGo.GetComponent<OverlayLayoutGroup>();
        buttonOlg.padding = new RectOffset(4, 4, 4, 4);

        var textComponent = GameObjectFactory.CreateSingleText(buttonTf, "Text", text);
        textComponent.enableWordWrapping = false;
        textComponent.fontSize = 18f;
        textComponent.horizontalAlignment = HorizontalAlignmentOptions.Left;

        return this;
    }

    /// <summary>
    /// Open the menu at the mouse cursor's location
    /// </summary>
    internal void ShowAtCursor()
    {
        // Get cursor position on the canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)_canvasGameObject.transform,
            Input.mousePosition,
            _canvasGameObject.GetComponent<Canvas>().worldCamera,
            out var cursorCanvasPos
        );

        // Position at the cursor
        var mainBodyTf = (RectTransform)_bodyGameObject.transform;
        mainBodyTf.position = _canvasGameObject.transform.TransformPoint(cursorCanvasPos);

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainBodyTf);
        var height = LayoutUtility.GetPreferredHeight(mainBodyTf);
        if (mainBodyTf.position.y - height < 0f)
        {
            // Show menu going upwards
            mainBodyTf.pivot = new Vector2(0f, 0f);
        }
        else
        {
            // Show menu going downwards
            mainBodyTf.pivot = new Vector2(0f, 1f);
        }

        // Animate it in
        mainBodyTf.localScale = new Vector3(0f, 0f, 1f);
        TootTallyAnimationManager.AddNewScaleAnimation(
            _bodyGameObject,
            Vector2.one,
            0.35f,
            new SecondDegreeDynamicsAnimation(3.5f, 1f, 1.2f)
        );
    }

    private void Hide()
    {
        if (_isHiding)
        {
            return;
        }
        _isHiding = true;

        // Animate it out
        TootTallyAnimationManager.AddNewScaleAnimation(
            _bodyGameObject,
            Vector2.zero,
            0.35f,
            new SecondDegreeDynamicsAnimation(3.5f, 1f, 1.2f)
        );

        // Prevent further interaction, so we can click on stuff underneath
        _canvasGameObject.GetComponent<CanvasGroup>().interactable = false;
        _canvasGameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // Destroy once the animation is over
        Object.Destroy(_canvasGameObject, 0.35f);
    }
}