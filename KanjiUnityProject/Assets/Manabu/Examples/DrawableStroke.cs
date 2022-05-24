using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;

namespace Manabu.Examples
{
    /// <summary>
    /// Holds the data relevant to a kanji stroke. Uses 2D coordinates.
    /// Designed to be used directly from the Kanji class.
    /// 
    /// All the poinst should be normalised to a 0-1 range.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class DrawableStroke : MonoBehaviour
    {
        // stats/data for the stroke
        public List<Vector2> refPoints; // key points in the stroke used for evaluation
        public List<Vector2> points;    // points used for visualising the line on screen
        private bool completed = false;
        public float length { get; protected set; }
        public bool isValid { get { return completed && refPoints?.Count == character.config.noRefPointsInStroke; } }

        // refs
        private LineRenderer line;
        private DrawableCharacter character;
        [SerializeField]
        protected Material lineMaterial;

        public void Awake()
        {
        }

        public void Init(DrawableCharacter character)
        {
            this.character = character;
            SetupLine();
        }

        public void AddPoint(Vector2 point)
        {
            points.Add(point);
            UpdateLinePoints();
        }

        public void AddPoints(List<Vector2> newPoints)
        {
            points.AddRange(newPoints);
            UpdateLinePoints();
        }

        public void Complete()
        {
            refPoints = SVGUtils.GenRefPntsForPnts(points, character.config.noRefPointsInStroke);
            length = SVGUtils.GetLengthForPnts(points);
            completed = true;
        }

        public void ClearLine()
        {
            points.Clear();
            UpdateLinePoints();
            completed = false;
        }

        public void SetupLine()
        {
            // hierarchy dependant
            character = GetComponentInParent<DrawableCharacter>();
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.material = lineMaterial;
            line.numCapVertices = 4;
            ResetLineWidth();
            ResetLineColor();
        }

        public void SetVisibility(bool visibility)
        {
            line.enabled = visibility;
        }

        public void UpdateLinePoints()
        {
            if (character.boxCollider == null) return;

            line.positionCount = points.Count;
            // set the normalised points to the size of the collider
            Vector3[] scaledPoints = new Vector3[points.Count];
            for (int i = 0; i < scaledPoints.Length; i++)
            {
                scaledPoints[i] = new Vector3(
                    points[i].x,
                    points[i].y,
                    // center of the box
                    character.kanjiZBoxRelativepos);
                scaledPoints[i].Scale(character.boxCollider.size);
            }
            line.SetPositions(scaledPoints);
        }

        protected void ResetLineColor()
        {
            line.startColor = lineColor;
            line.endColor = lineColor;
        }

        protected void SetLineColorTemp(Color color)
        {
            line.startColor = color;
            line.endColor = color;
        }

        protected void SetLineWidthTemp(float width)
        {
            line.startWidth = width;
            line.endWidth = width;
        }

        protected void ResetLineWidth()
        {
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
        }

        // persistent line configuration
        protected Color _lineColor = Color.red;
        public Color lineColor { get { return _lineColor; } set { _lineColor = value; ResetLineColor(); } }
        protected float _lineWidth = 0.1f;
        public float lineWidth { get { return _lineWidth; } set { _lineWidth = value; ResetLineWidth(); } }

        #region Highlights

        protected struct HighlightData
        {
            public float fromWidth;
            public Color fromColor;
            public Coroutine co;
        }

        protected HighlightData highlightData;


        public void SetHightlight(Color color, float widthScale = 2)
        {
            highlightData.fromColor = color;
            highlightData.fromWidth = lineWidth * widthScale;
        }

        public void Highlight()
        {
            if (highlightData.co != null) StopCoroutine(highlightData.co);
            // reset the state of the line
            ResetLineColor();
            ResetLineWidth();
            highlightData.co = StartCoroutine(ApplyHighlight());
        }

        private IEnumerator ApplyHighlight()
        {
            //Debug.Log("Running highlight coroutine");
            for (float highlightAlpha = 0; highlightAlpha <= 1; highlightAlpha += 0.05f)
            {
                SetLineColorTemp(Color.Lerp(highlightData.fromColor, lineColor, highlightAlpha));
                SetLineWidthTemp(Mathf.Lerp(highlightData.fromWidth, lineWidth, highlightAlpha));
                yield return new WaitForSeconds(0.01f);
            }
            ResetLineColor();
            ResetLineWidth();
        }

        #endregion

    }

}