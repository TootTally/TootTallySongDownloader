using UnityEngine;
using UnityEngine.UI;

namespace TootTallySongDownloader.Ui;

/// <summary>
/// It's a progress bar, but not a bar but a pie wheel instead
/// </summary>
/// <remarks>
/// At some point this might be moved out of song downloader if other TootTally modules want it
/// </remarks>
public class ProgressPieGraphic : MaskableGraphic
{
    internal float FillPercent {
        get => _fillPercent;
        set
        {
            _fillPercent = value;
            SetVerticesDirty();
        }
    }
    private float _fillPercent = 0f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        const float tau = Mathf.PI * 2f;
        const float step = 0.025f;

        vh.Clear();

        var rect = rectTransform.rect;
        var center = rect.center;
        var radius = Mathf.Min(rect.width, rect.height) / 2f;

        // Vert index 0 is always the center vertex
        var centerVert = new UIVertex
        {
            position = center,
            color = color,
        };
        vh.AddVert(centerVert);

        for (var i = 0f; i < tau * FillPercent; i += step)
        {
            var vert1Index = vh.currentVertCount;
            var vert2Index = vert1Index + 1;

            var pos1 = new Vector2(Mathf.Sin(i), Mathf.Cos(i)) * radius;
            var pos2 = new Vector2(Mathf.Sin(i + step), Mathf.Cos(i + step)) * radius;

            var vert1 = new UIVertex
            {
                position = pos1,
                color = color,
            };
            var vert2 = new UIVertex
            {
                position = pos2,
                color = color,
            };

            vh.AddVert(vert1);
            vh.AddVert(vert2);
            vh.AddTriangle(0, vert1Index, vert2Index);
        }
    }
}
