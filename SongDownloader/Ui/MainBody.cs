#nullable enable

using System;
using TMPro;
using TootTallyCore.Graphics;
using TootTallyCore.Utils.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

public class MainBody
{
    private readonly GameObject _gameObject;
    private RectTransform Transform => (RectTransform)_gameObject.transform;

    private readonly TMP_Text _songNameText;
    private readonly TMP_Text _artistText;
    private readonly StatDisplay _durationStatDisplay;
    private readonly StatDisplay _difficultyStatDisplay;
    private readonly StatDisplay _charterStatDisplay;

    /// <summary>
    /// Private ctor, use <c>Create</c> instead
    /// </summary>
    private MainBody(
        GameObject gameObject,
        TMP_Text songNameText,
        TMP_Text artistText,
        StatDisplay durationStatDisplay,
        StatDisplay difficultyStatDisplay,
        StatDisplay charterStatDisplay
    )
    {
        _gameObject = gameObject;
        _songNameText = songNameText;
        _artistText = artistText;
        _durationStatDisplay = durationStatDisplay;
        _difficultyStatDisplay = difficultyStatDisplay;
        _charterStatDisplay = charterStatDisplay;
    }

    internal static MainBody Create()
    {
        // Make main body //////////////////////////////////////////////////////////////////////////////////////////////
        var bodyGo = new GameObject(
            "MainBody",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster),
            typeof(LayoutElement)
        );

        var mainBodyTf = (RectTransform)bodyGo.transform;
        // mainBodyTf.SetParent(songRowTf, false); // TODO

        var mainBodyLayoutElem = bodyGo.GetComponent<LayoutElement>();
        mainBodyLayoutElem.flexibleWidth = 1f;

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
        var panelBorder = new GameObject(
            "PanelBorder",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        var panelBorderTf = (RectTransform)panelBorder.transform;
        panelBorderTf.SetParent(mainBodyTf, false);
        panelBorderTf.anchorMin = Vector2.zero;
        panelBorderTf.anchorMax = Vector2.one;
        panelBorderTf.offsetMin = Vector2.zero;
        panelBorderTf.offsetMax = Vector2.zero;

        var panelBorderImage = panelBorder.GetComponent<Image>();
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

        // Add main body left text /////////////////////////////////////////////////////////////////////////////////////
        var songNameText = GameObjectFactory.CreateSingleText(mainBodyTf, "SongName", "");
        var songNameTf = songNameText.rectTransform!;
        songNameTf.anchorMin = new Vector2(0f, 1f);
        songNameTf.anchorMax = Vector2.one;
        songNameTf.offsetMin = new Vector2(8f, -28f - 8f);
        songNameTf.offsetMax = new Vector2(-200f - 8f, 0f);
        songNameText.fontSize = 24f;
        songNameText.alignment = TextAlignmentOptions.BottomLeft;
        songNameText.overflowMode = TextOverflowModes.Ellipsis;
        songNameText.enableWordWrapping = false;

        var artistText = GameObjectFactory.CreateSingleText(mainBodyTf, "Artist", "");
        var artistTf = artistText.rectTransform!;
        artistTf.anchorMin = Vector2.zero;
        artistTf.anchorMax = new Vector2(1f, 0f);
        artistTf.offsetMin = Vector2.one * 8f;
        artistTf.offsetMax = new Vector2(-200f - 8f, 50f + 8f);
        artistText.fontSize = 18f;
        artistText.alignment = TextAlignmentOptions.BottomLeft;
        artistText.overflowMode = TextOverflowModes.Ellipsis;
        artistText.enableWordWrapping = false;
        artistText.color = new Color(1f, 1f, 1f, 0.5f);

        // Add main body right top box /////////////////////////////////////////////////////////////////////////////////
        var rightTopBox = new GameObject(
            "RightTopBox",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );

        var rightTopBoxTf = (RectTransform)rightTopBox.transform;
        rightTopBoxTf.SetParent(mainBodyTf, false);
        rightTopBoxTf.anchorMin = new Vector2(1f, 0f);
        rightTopBoxTf.anchorMax = Vector2.one;
        rightTopBoxTf.offsetMin = new Vector2(-200 -8f, 8f);
        rightTopBoxTf.offsetMax = Vector2.one * -8f;

        var rightTopBoxLayout = rightTopBox.GetComponent<HorizontalLayoutGroup>();
        rightTopBoxLayout.childControlWidth = true;
        rightTopBoxLayout.childControlHeight = true;
        rightTopBoxLayout.childForceExpandWidth = false;
        rightTopBoxLayout.childForceExpandHeight = false;
        rightTopBoxLayout.childAlignment = TextAnchor.UpperRight;
        rightTopBoxLayout.spacing = 8f;

        var durationStatDisplay = StatDisplay.Create().WithParent(rightTopBoxTf).WithIcon("time64.png");
        var difficultyStatDisplay = StatDisplay.Create().WithParent(rightTopBoxTf).WithIcon("stardiff64.png");

        // Add main body right bottom box //////////////////////////////////////////////////////////////////////////////
        var rightBottomBox = new GameObject(
            "RightBottomBox",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );

        var rightBottomBoxTf = (RectTransform)rightBottomBox.transform;
        rightBottomBoxTf.SetParent(mainBodyTf, false);
        rightBottomBoxTf.anchorMin = new Vector2(1f, 0f);
        rightBottomBoxTf.anchorMax = Vector2.one;
        rightBottomBoxTf.offsetMin = new Vector2(-200 -8f, 8f);
        rightBottomBoxTf.offsetMax = Vector2.one * -8f;

        var rightBottomBoxLayout = rightBottomBox.GetComponent<HorizontalLayoutGroup>();
        rightBottomBoxLayout.childControlWidth = true;
        rightBottomBoxLayout.childControlHeight = true;
        rightBottomBoxLayout.childForceExpandWidth = false;
        rightBottomBoxLayout.childForceExpandHeight = false;
        rightBottomBoxLayout.childAlignment = TextAnchor.LowerRight;
        rightBottomBoxLayout.spacing = 8f;

        var charterStatDisplay = StatDisplay.Create().WithParent(rightBottomBoxTf).WithIcon("Pencil64.png");

        // TODO: Add ranked icon somewhere

        return new MainBody(
            gameObject: bodyGo,
            songNameText: songNameText,
            artistText: artistText,
            durationStatDisplay: durationStatDisplay,
            difficultyStatDisplay: difficultyStatDisplay,
            charterStatDisplay: charterStatDisplay
        );
    }

    internal MainBody WithParent(Transform parent)
    {
        Transform.SetParent(parent, false);
        return this;
    }

    internal MainBody WithSongName(string text)
    {
        _songNameText.text = text;
        return this;
    }

    internal MainBody WithArtist(string text)
    {
        _artistText.text = text;
        return this;
    }

    internal MainBody WithDurationSeconds(float seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        var stringTime = $"{(time.Hours != 0 ? (time.Hours + ":") : "")}{(time.Minutes != 0 ? time.Minutes : "0")}:{(time.Seconds != 0 ? time.Seconds : "00"):00}";
        _durationStatDisplay.WithText(stringTime);
        return this;
    }

    internal MainBody WithDifficulty(float difficulty)
    {
        _difficultyStatDisplay.WithText(difficulty.ToString("n2"));
        return this;
    }

    internal MainBody WithCharter(string charterDisplayName)
    {
        _charterStatDisplay.WithText(charterDisplayName);
        return this;
    }
}
