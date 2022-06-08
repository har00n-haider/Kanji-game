using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;
using System;

public class CharacterStrokeFollowTarget : MonoBehaviour
{
    // timing
    [SerializeField]
    private double hangAboutTime;
    public double BeatTimeStamp { get { return beat.timestamp; } }
    private double startTimeStamp = 0;
    private Beat beat;
    public Beat Beat { get { return beat; } }

    // beat circle
    [SerializeField]
    private LineRenderer beatCircleLine;
    private Vector3[] beatCirclePoints = new Vector3[40];
    private float beatCircleRadiusBegin;
    private float beatCircleRadiusEnd;
    private float beatCircleLineWidth;
    public bool BeatCircleActive { get { return beatCircleLine.gameObject.activeInHierarchy; } private set { beatCircleLine.gameObject.SetActive(value); } }

    // range circle
    [SerializeField]
    private LineRenderer rangeCircleLine;
    private Vector3[] rangeCirclePoints = new Vector3[40];
    public bool RangeCircleActive { get { return rangeCircleLine.gameObject.activeInHierarchy; } set { rangeCircleLine.gameObject.SetActive(value); } }
    private float rangeCircleLineWidth;
    private float rangeCircleRadius;


    // model
    [SerializeField]
    private GameObject model;
    private Renderer modelRenderer;
    private SphereCollider modelCollider;
    public SphereCollider Collider { get { return modelCollider; } }


    public void Init(Beat beat, CharacterConfig config)
    {
        this.beat = beat;
        modelCollider = model.GetComponent<SphereCollider>();
        startTimeStamp = AudioSettings.dspTime;

        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = false;
        beatCircleLine.numCapVertices = 10;
        beatCircleLineWidth = config.followTargetBeatCircleLineWidth;
        beatCircleLine.endWidth = beatCircleLineWidth;
        beatCircleLine.startWidth = beatCircleLineWidth;
        beatCircleRadiusEnd = 0.5f; // default radius for cylinder primitive
        beatCircleRadiusBegin = config.followTargetBeatCircleRadiusBegin;
        BeatCircleActive = true;

        modelCollider.radius = config.followTargetColliderRadius;
        rangeCircleRadius = config.followTargetColliderRadius;
        rangeCircleLine.positionCount = rangeCirclePoints.Length;
        rangeCircleLine.useWorldSpace = false;
        rangeCircleLine.numCapVertices = 10;
        rangeCircleLineWidth = config.followTargetRangeCircleLineWidth;
        rangeCircleLine.endWidth = rangeCircleLineWidth;
        rangeCircleLine.startWidth = rangeCircleLineWidth;
        RangeCircleActive = false;
        UpdateRangeCircle();
    }

    void Update()
    {
        UpdateBeatCircle();
    }


    public bool HitCheck()
    {
        if (this == null) return false;
        Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition());
        return modelCollider.Raycast(ray, out RaycastHit hit, float.MaxValue);
    }

    private void UpdateRangeCircle()
    {
        float radius = rangeCircleRadius;
        GeometryUtils.PopulateCirclePoints3DXY(ref rangeCirclePoints, 
            radius, 
            Vector3.zero);
        for (int i = 0; i < rangeCirclePoints.Length; i++)
        {
            rangeCircleLine.SetPosition(i, rangeCirclePoints[i]);
        }
    }

    private void UpdateBeatCircle()
    {
        if(BeatCircleActive)
        {
            // decrease size of the beat circle based on time elapsed
            float t = (float)MathUtils.InverseLerp(beat.timestamp, startTimeStamp, AudioSettings.dspTime);
            float radius = Mathf.Lerp(beatCircleRadiusEnd, beatCircleRadiusBegin, t);
            GeometryUtils.PopulateCirclePoints3DXY(ref beatCirclePoints, 
                radius, 
                Vector3.zero);
            for (int i = 0; i < beatCirclePoints.Length; i++)
            {
                beatCircleLine.SetPosition(i, beatCirclePoints[i]);
            }
            if (t <= 0) BeatCircleActive = false;
        }
    }

    public void SetColor(Color color)
    {
        if (modelRenderer == null) modelRenderer = model.GetComponent<Renderer>();
        if (modelRenderer.material.color == color) return;
        modelRenderer.material.color = color;
    }


}
