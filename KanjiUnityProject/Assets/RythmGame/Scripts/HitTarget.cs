using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using KanjiLib.Prompts;

public class HitTarget : MonoBehaviour
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
    private Color questionColor;
    [SerializeField]
    private Color answerColor;
    [SerializeField]
    private Color waitingAnswerColor;

    public enum Result 
    {
        Miss,
        Hit
    }

    // text
    [SerializeField]
    private TextMeshPro textMesh;

    // timing
    [SerializeField]
    private double hangAboutTime;
    private double beatTimeStamp = 0;
    public double BeatTimeStamp { get { return beatTimeStamp;} }
    private double startTimeStamp = 0;

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
    [SerializeField]
    private CapsuleCollider modelCollider;
    [SerializeField]
    private GameObject model;
    private Renderer modelRenderer;

    // prompt stuff
    public PromptChar prompt;
    public Type type;
    public HitTargetSpawner.HitGroup group;
    public bool selected = false;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(double beatTimeStamp, Type type,  PromptChar prompt, HitTargetSpawner.HitGroup group)  
    {
        startTimeStamp = AudioSettings.dspTime;
        this.beatTimeStamp = beatTimeStamp;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; 
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        radiusEnd = modelCollider.radius;

        // prompt stuff
        textMesh.text = prompt.GetDisplaySstring();
        this.prompt = prompt;
        this.type = type;
        this.group = group;
        if(type == Type.Question)
        {
            SetModelColor(questionColor);
        }
        else
        {
            SetModelColor(answerColor);
        }
    }


    void Awake()
    {

    }

    void Update()
    {
        bool thresholdPassed = AudioSettings.dspTime >
            beatTimeStamp + GameManager.Instance.GameAudio.BeatManager.BeatHitAllowance * 1.2f;
        if (thresholdPassed && type != Type.Question) HandleBeatResult(Result.Miss);

        UpdateBeatCircle();
    }

    public void HandleBeatResult(Result hitResult)
    {
        if (hitResult == Result.Hit)
        {
            if (type == Type.Question && !selected)
            {
                SetModelColor(waitingAnswerColor);
                Instantiate(selectedEffect, transform.position, Quaternion.identity);
                AppEvents.OnSelected?.Invoke(this);
                selected = true;
            }
            else if(type == Type.Answer && group.question.selected)
            {
                bool promptResult = group.question.prompt.Check(prompt);
                if (promptResult) 
                {
                    group.question.HandlePromptResult(ResultAction.Success);
                    HandlePromptResult(ResultAction.Success);
                    group.answers.Remove(this);
                    group.answers.ForEach(a => a.HandlePromptResult(ResultAction.Nothing));
                }
                else
                {
                    group.question.HandlePromptResult(ResultAction.Failiure);
                    HandlePromptResult(ResultAction.Failiure);
                    group.answers.Remove(this);
                    group.answers.ForEach(a => a.HandlePromptResult(ResultAction.Nothing));
                }
            }
        }
        else
        {
            Instantiate(failEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
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
        float t = (float) MathUtils.InverseLerp(beatTimeStamp, startTimeStamp, AudioSettings.dspTime) ;
        float radius = Mathf.Lerp(radiusEnd, radiusBegin, t);
        GeometryUtils.PopulateCirclePoints3DXY(ref beatCirclePoints, radius, transform.position);
        for (int i = 0; i < beatCirclePoints.Length; i++)
        {
            beatCircleLine.SetPosition(i, beatCirclePoints[i]);
        }
    }

    private void SetModelColor(Color color)
    {
        if(modelRenderer == null) modelRenderer = model.GetComponent<Renderer>();
        modelRenderer.material.color = color;
    }


}
