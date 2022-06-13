using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using System;

public class BasicTargetSpawnData
{
    public Beat beat;
    public Vector3 position;
    public bool spawned;
}

public class BasicTarget : MonoBehaviour
{

    [SerializeField]
    private Color beatWindowColor;
    [SerializeField]
    private Color targetColor;

    // timing
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

    //refs
    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }
    private BasicTargetConfig config;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(Beat beat, BasicTargetConfig config)  
    {
        startTimeStamp = AudioSettings.dspTime;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; 
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        this.beat = beat;
        modelColor = targetColor;
        modelCollider = model.GetComponent<CapsuleCollider>();
        radiusEnd = modelCollider.radius;
        transform.localScale = transform.localScale * config.targetScale;
        hangAboutTime = config.hangaboutTime;
        this.config = config;
    }

    void Update()
    {
        if (beatManager.IsBeatMissed(beat, config.beatMissedThreshold))
        {
            HandleBeatResult(BeatResult.Miss);
        };


        UpdateBeatCircle();

        CheckInput();
    }

    private void CheckInput()
    {
        if (this == null) return;

        if(GameInput.GetButton1Down())
        {
            Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition());
            if(modelCollider.Raycast(ray, out RaycastHit hit, float.MaxValue))
            {
                HandleBeatResult(BeatResult.Hit);
            }
            else
            {
                HandleBeatResult(BeatResult.Miss);
            };
        }
    }

    private void HandleBeatResult(BeatResult hitResult)
    {
        if(this == null) return;

        switch (hitResult)
        {
            case BeatResult.Hit:
                Instantiate(succesEffect, transform.position, Quaternion.identity);
                break;
            case BeatResult.Miss:
                Instantiate(failEffect, transform.position, Quaternion.identity);
                break;
        }
        Destroy(gameObject);
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
        bool onBeat = GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(beat);
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
