using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

/// <summary>
/// Overlay items on top of each other. This takes the size of the largest child item's sizes.
/// </summary>
public class OverlayLayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup
{
    public float minWidth { private set; get; }
    public float preferredWidth { private set; get; }
    public float flexibleWidth { private set; get; }
    public float minHeight { private set; get; }
    public float preferredHeight { private set; get; }
    public float flexibleHeight { private set; get; }
    public int layoutPriority => 0;

    public RectOffset padding = new();

    [System.NonSerialized] private RectTransform _rectTransform;
    private RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public void CalculateLayoutInputHorizontal()
    {
        minWidth = 0f;
        preferredWidth = 0f;
        flexibleWidth = 0f;

        for (var i = 0; i < RectTransform.childCount; i++)
        {
            var childTf = (RectTransform)RectTransform.GetChild(i).transform;

            minWidth = Mathf.Max(minWidth, LayoutUtility.GetMinWidth(childTf));
            preferredWidth = Mathf.Max(preferredWidth, LayoutUtility.GetPreferredWidth(childTf));
            flexibleWidth = Mathf.Max(flexibleWidth, LayoutUtility.GetFlexibleWidth(childTf));
        }

        var horizPadding = padding.left + padding.right;
        minWidth += horizPadding;
        preferredWidth += horizPadding;
        flexibleWidth += horizPadding;
    }

    public void CalculateLayoutInputVertical()
    {
        minHeight = 0f;
        preferredHeight = 0f;
        flexibleHeight = 0f;

        for (var i = 0; i < RectTransform.childCount; i++)
        {
            var childTf = (RectTransform)RectTransform.GetChild(i).transform;

            minHeight = Mathf.Max(minHeight, LayoutUtility.GetMinHeight(childTf));
            preferredHeight = Mathf.Max(preferredHeight, LayoutUtility.GetPreferredHeight(childTf));
            flexibleHeight = Mathf.Max(flexibleHeight, LayoutUtility.GetFlexibleHeight(childTf));
        }

        var vertPadding = padding.bottom + padding.top;
        minHeight += vertPadding;
        preferredHeight += vertPadding;
        flexibleHeight += vertPadding;
    }

    public void SetLayoutHorizontal()
    {
        for (var i = 0; i < RectTransform.childCount; i++)
        {
            var childTf = (RectTransform)RectTransform.GetChild(i).transform;

            childTf.anchorMin = new Vector2(0f, childTf.anchorMin.y);
            childTf.anchorMax = new Vector2(1f, childTf.anchorMax.y);
            childTf.offsetMin = new Vector2(padding.left, childTf.offsetMin.y);
            childTf.offsetMax = new Vector2(-padding.right, childTf.offsetMax.y);
        }
    }

    public void SetLayoutVertical()
    {
        for (var i = 0; i < RectTransform.childCount; i++)
        {
            var childTf = (RectTransform)RectTransform.GetChild(i).transform;

            childTf.anchorMin = new Vector2(childTf.anchorMin.x, 0f);
            childTf.anchorMax = new Vector2(childTf.anchorMax.x, 1f);
            childTf.offsetMin = new Vector2(childTf.offsetMin.x, padding.bottom);
            childTf.offsetMax = new Vector2(childTf.offsetMax.x, -padding.top);
        }
    }
}
