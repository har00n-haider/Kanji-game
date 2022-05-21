using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;

public class DrawTarget : MonoBehaviour
{
    public enum Type
    {
        Question,
        Answer
    }

    public enum ResultAction
    {
        Success,
        Failiure,
        Nothing
    }

    [SerializeField]
    private Color beatWindowColor;
    [SerializeField]
    private Color questionColor;
    [SerializeField]
    private Color answerColor;
    [SerializeField]
    private Color selectedColor;

    // text
    [SerializeField]
    private TextMeshPro textMesh;

    // timing
    [SerializeField]
    private double hangAboutTime;
    public double BeatTimeStamp { get { return beat.timestamp;} }
    private double startTimeStamp = 0;
    private BeatManager.Beat beat;
    public BeatManager.Beat Beat { get { return beat; } }

    // Effects
    [SerializeField]
    private GameObject succesEffect;
    [SerializeField]
    private GameObject failEffect;
    [SerializeField]
    private GameObject selectedEffect;

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
    private CapsuleCollider modelCollider;
    [SerializeField]
    private GameObject model;
    private Renderer modelRenderer;
    private Color modelColor;

    // prompt stuff
    public Character prompt;
    public Type type;
    public TargetSpawner.KanaReadingGroup group;
    public bool selected = false;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(Type type, Character character, TargetSpawner.KanaReadingGroup group, BeatManager.Beat beat)  
    {
        startTimeStamp = AudioSettings.dspTime;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; 
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        radiusEnd = modelCollider.radius;

        this.beat = beat;

        // prompt stuff
        textMesh.text = character.GetDisplayString();
        this.prompt = character;
        this.type = type;
        this.group = group;
        if(type == Type.Question)
        {
            modelColor = questionColor;
        }
        else
        {
            modelColor = answerColor;
        }
    }


    void Awake()
    {

    }

    void Update()
    {
        bool thresholdPassed = AudioSettings.dspTime >
            beat.timestamp + GameManager.Instance.GameAudio.BeatManager.BeatHitAllowance * 1.2f;
        //if (thresholdPassed) HandleBeatResult(Result.Miss);

        UpdateBeatCircle();
        UpdateColor();
    }


    public void HandlePromptResult(ResultAction action)
    {
        if(this == null) return;

        switch (action)
        {
            case ResultAction.Success:
                Instantiate(succesEffect, transform.position, Quaternion.identity);
                break;
            case ResultAction.Failiure:
                Instantiate(failEffect, transform.position, Quaternion.identity);
                break;
            case ResultAction.Nothing:
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

    private void UpdateColor()
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
