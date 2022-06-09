using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;
using System;

public class ReadContainerTarget : MonoBehaviour
{

    [SerializeField]
    private Color beatWindowColor;
    [SerializeField]
    private Color targetColor;

    // timing
    [SerializeField]
    private double hangAboutTime;
    public double BeatTimeStamp { get { return beat.timestamp;} }
    private double startTimeStamp = 0;
    private Beat beat;
    public Beat Beat { get { return beat; } }

    // Effects
    [SerializeField]
    private GameObject succesEffect;
    [SerializeField]
    private GameObject failEffect;

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
    private CapsuleCollider modelCollider;
    private Color modelColor;

    // lifetime
    Action<Beat> onDestroyCallback;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(Beat beat, BasicTargetConfig config, Action<Beat> onDestroyCallback)  
    {
        startTimeStamp = AudioSettings.dspTime;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; 
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        this.beat = beat;
        this.onDestroyCallback = onDestroyCallback;
        modelColor = targetColor;
        modelCollider = model.GetComponent<CapsuleCollider>();
        radiusEnd = modelCollider.radius;
        transform.localScale = transform.localScale * config.targetScale;
    }

    void Update()
    {
        bool thresholdPassed = AudioSettings.dspTime >
            beat.timestamp + GameManager.Instance.GameAudio.BeatManager.BeatHitAllowance * 1.2f;
        if (thresholdPassed) HandleBeatResult(Result.Miss);
        UpdateBeatCircle();
        UpdateBeatWindowColor();
    }


    public bool HitCheck()
    {
        if (this == null) return false;
        Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition());
        return modelCollider.Raycast(ray, out RaycastHit hit, float.MaxValue);
    }


    public void HandleBeatResult(Result hitResult)
    {
        if(this == null) return;

        switch (hitResult)
        {
            case Result.Hit:
                Instantiate(succesEffect, transform.position, Quaternion.identity);
                break;
            case Result.Miss:
                Instantiate(failEffect, transform.position, Quaternion.identity);
                break;
        }
        Destroy(gameObject);
        onDestroyCallback?.Invoke(beat);
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

    private void SetModelColor(Color color)
    {
        if (modelRenderer == null) modelRenderer = model.GetComponent<Renderer>();
        if (modelRenderer.material.color == color) return;
        modelRenderer.material.color = color;
    }

    private void UpdateBeatWindowColor()
    {
        bool onBeat = GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(beat.timestamp);
        if (onBeat )
        {
            SetModelColor(beatWindowColor);
        }
        else
        {
            SetModelColor(modelColor);
        }
    }

}
