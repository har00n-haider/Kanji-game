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
    public double BeatTimeStamp { get { return beat.timestamp;} }
    private double startTimeStamp = 0;
    private BeatManager.Beat beat;
    public BeatManager.Beat Beat { get { return beat; } }

    // beat circle
    [SerializeField]
    private LineRenderer beatCircleLine;
    private Vector3[] beatCirclePoints = new Vector3[40];
    [SerializeField]
    private float radiusBegin;
    private float radiusEnd;
    private float beatCircleLineWidth = 0.1f;

    // model
    [SerializeField]
    private GameObject model;
    private Renderer modelRenderer;
    private SphereCollider modelCollider;
    private Color modelColor;
    public SphereCollider Collider { get { return modelCollider; } }

    // lifetime
    Action<BeatManager.Beat> onDestroyCallback;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(BeatManager.Beat beat, Action<BeatManager.Beat> onDestroyCallback)  
    {
        startTimeStamp = AudioSettings.dspTime;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; 
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        this.beat = beat;
        this.onDestroyCallback = onDestroyCallback;
        modelCollider = model.GetComponent<SphereCollider>();
        radiusEnd = modelCollider.radius;
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


    private void UpdateBeatCircle() 
    {
        // decrease size of the beat circle based on time elapsed
        float t = (float) MathUtils.InverseLerp(beat.timestamp, startTimeStamp, AudioSettings.dspTime) ;
        float radius = Mathf.Lerp(radiusEnd, radiusBegin, t);
        GeometryUtils.PopulateCirclePoints3DXY(ref beatCirclePoints, radius, transform.position);
        for (int i = 0; i < beatCirclePoints.Length; i++)
        {
            beatCircleLine.SetPosition(i, beatCirclePoints[i]);
        }
    }

    public void SetColor(Color color)
    {
        if (modelRenderer == null) modelRenderer = model.GetComponent<Renderer>();
        if (modelRenderer.material.color == color) return;
        modelRenderer.material.color = color;
    }


}
