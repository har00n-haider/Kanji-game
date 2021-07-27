using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ReferenceStroke : Stroke
{
    private struct HighlightData
    {
        public float initialWidth;
        public float finalWidth;
        public Color initialColor;
        public Color finalColor;
        public Coroutine co;
    }

    private HighlightData highlightData;

    public RawStroke rawStroke;

    public override void Init(Kanji kanji)
    {
        base.Init(kanji);
        base.SetupLine(Color.grey);
        // use the raw kanji data to create lines in the world
        line.positionCount = rawStroke.points.Count;
        line.SetPositions(rawStroke.points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
        line.useWorldSpace = false;
        refPoints = Utils.GenRefPntsForPnts(rawStroke.points);

        // highlight configuration
        highlightData.finalColor = line.startColor;
        highlightData.finalWidth = line.startWidth;
        highlightData.initialColor = Color.red;
        highlightData.initialWidth = width * 3;

    }

    #region Highlight

    public void Highlight()
    {
        if(highlightData.co != null) StopCoroutine(highlightData.co);
        SetLineColor(highlightData.finalColor);
        SetLineWidth(highlightData.finalWidth); highlightData.co = StartCoroutine(ApplyHighlight());
    }

    private IEnumerator ApplyHighlight()
    {
        for (float highlightAlpha = 0; highlightAlpha <= 1; highlightAlpha += 0.05f)
        {
            Debug.Log("ran coroutine once");
            SetLineColor(Color.Lerp(highlightData.initialColor, highlightData.finalColor, highlightAlpha));
            SetLineWidth(Mathf.Lerp(highlightData.initialWidth, highlightData.finalWidth, highlightAlpha));
            yield return new WaitForSeconds(0.01f);
        }
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach(Vector2 pnt in refPoints) 
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(kanji.gameObject.transform.
                TransformPoint(new Vector3(pnt.x, pnt.y)), 0.1f);
        }
    }
#endif

}

