using Manabu.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CharacterPassedEffect : CustomEffect
{

    [SerializeField]
    private GameObject strokePrefab;

    private Vector2 size;
    private CharacterTarget charTarget;
    private CharacterStrokeConfig config;
    private Color color;
    private float initialWidth;
    private List<LineRenderer> strokes = new();

    public void Init(Vector2 size, CharacterTarget charTarget, CharacterStrokeConfig config, Color initialColor)
    {
        this.size = size;
        this.charTarget = charTarget;
        this.config = config;
        this.color = initialColor;
    }

    public override void Play()
    {
        for (int i = 0; i < charTarget.Character.drawData.strokes.Count; i++)
        {
            GameObject gameObject = Instantiate(strokePrefab, transform.position, Quaternion.identity, transform);

            //Add Components
            var strokeLine = gameObject.GetComponent<LineRenderer>();

            // generate stroke
            var refStroke = new ReferenceStroke(size, charTarget.Character.drawData.strokes[i], config);

            // setup the line renderer to display a line connecting them
            // everything else is set in the component in the editor
            strokeLine.useWorldSpace = false;
            strokeLine.positionCount = refStroke.points.Count;
            strokeLine.SetPositions(refStroke.points.ConvertAll((p) => new Vector3(p.x, p.y, charTarget.CharacterCenter.z)).ToArray());
            strokeLine.startWidth = config.lineWidth;
            strokeLine.endWidth = config.lineWidth;
            strokeLine.startColor = color;
            strokeLine.endColor = color;

            strokes.Add(strokeLine);
        }

        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        float alpha = 1f;
        float dWidth = 0.03f;

        while(alpha >= 0)
        {
            alpha -= 0.007f;
            color.a = alpha;
            foreach (var line in strokes)
            {
                line.startColor = color;
                line.endColor = color;
                line.startWidth += dWidth;
                line.endWidth += dWidth;


            }

            yield return null;
        }

    }

}