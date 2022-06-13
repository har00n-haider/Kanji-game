using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using System;



public class ReadContainerTarget : MonoBehaviour
{

    public enum Type
    {
        Question,
        Answer
    }

    public enum AnswerResult
    {
        Success,
        Failiure,
        Nothing
    }

    // state 
    public Type type;
    public bool selected = false;
    private Character character;
    public AnswerResult result = AnswerResult.Nothing;


    // timing
    public double BeatTimeStamp { get { return beat.timestamp; } }
    private double startTimeStamp = 0;
    private Beat beat;
    public Beat Beat { get { return beat; } }

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
    private float radiusBegin;
    private float radiusEnd;

    // model
    [SerializeField]
    private GameObject model;
    private Renderer modelRenderer;
    private CapsuleCollider modelCollider;

    // text
    [SerializeField]
    private TextMeshPro textMesh;

    // lifetime
    Action onSelected;

    // refs
    [SerializeField]
    private Color questionColor;
    [SerializeField]
    private Color answerColor;
    [SerializeField]
    private Color selectedColor;
    private ReadTarget readTarget;
    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }
    private ReadTargetConfig config;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(Beat beat,
        ReadTarget readTarget,
        Character character,
        Type type,
        ReadTargetConfig config,
        ReadTargetSpawnData readTargetData,
        Action onSelected)
    {
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = false;
        beatCircleLine.numCapVertices = 10;
        beatCircleLine.endWidth = config.lineWidth;
        beatCircleLine.startWidth = config.lineWidth;
        modelCollider = model.GetComponent<CapsuleCollider>();
        radiusEnd = 0.5f;
        radiusBegin = config.beatCircleRadiusBegin;

        SetModelColor(type == Type.Answer ? answerColor : questionColor);

        transform.localScale = transform.localScale * config.targetScale;
        startTimeStamp = beatManager.timeIntoSong;

        if(type == Type.Answer) textMesh.text = readTargetData.kanaToRomaji ? character.romaji : character.literal.ToString();
        if(type == Type.Question) textMesh.text = !readTargetData.kanaToRomaji ? character.romaji : character.literal.ToString();
        this.beat = beat;
        this.onSelected = onSelected;
        this.character = character;
        this.readTarget = readTarget;
        this.config = config;
        this.type = type;
    }

    void Update()
    {
        UpdateBeatCircle();
        CheckInput();
    }


    private void CheckInput()
    {
        if (this == null) return;

        if (GameInput.GetButton1Down())
        {
            Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition());
            if (modelCollider.Raycast(ray, out RaycastHit hit, float.MaxValue))
            {
                if (beatManager.CheckIfOnBeat(beat))
                {
                    HandleBeatResult(BeatResult.Hit);
                }
                else
                {
                    HandleBeatResult(BeatResult.Miss);
                };
            }
        }
    }

    public void HandleBeatResult(BeatResult hitResult)
    {
        if (this == null) return;

        switch (hitResult)
        {
            case BeatResult.Hit:
                if (type == Type.Question && !selected)
                {
                    SetModelColor(selectedColor);
                    Instantiate(selectedEffect, transform.position, Quaternion.identity);
                    selected = true;
                    onSelected?.Invoke();
                }
                else if (type == Type.Answer && readTarget.question.selected)
                {
                    // check if the current answer is correct
                    result = readTarget.question.character.Check(character) ? AnswerResult.Success : AnswerResult.Failiure;
                    HandleReadResult(result);
                }
                break;
            case BeatResult.Miss:
                if (type == Type.Question && !selected)
                {

                    Instantiate(failEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);

                }
                else if (type == Type.Answer)
                {
                    Instantiate(failEffect, transform.position, Quaternion.identity);
                    HandleReadResult(AnswerResult.Nothing);
                }
                break;
        }
    }

    public void HandleReadResult(AnswerResult result)
    {
        switch (result)
        {
            case AnswerResult.Success:
                Instantiate(succesEffect, transform.position, Quaternion.identity);
                break;
            case AnswerResult.Failiure:
                Instantiate(failEffect, transform.position, Quaternion.identity);
                break;
        }
        foreach (var a in readTarget.answers)
        {
            Destroy(a.gameObject);
        }
        Destroy(readTarget.question.gameObject);
        Destroy(readTarget.gameObject, 2);
    }

    private void UpdateBeatCircle()
    {
        // decrease size of the beat circle based on time elapsed
        float t = (float)MathUtils.InverseLerp(beat.timestamp, startTimeStamp, beatManager.timeIntoSong);
        float radius = Mathf.Lerp(radiusEnd, radiusBegin, t);
        GeometryUtils.PopulateCirclePoints3DXY(ref beatCirclePoints, 
            radius, 
            Vector3.zero);
        for (int i = 0; i < beatCirclePoints.Length; i++)
        {
            beatCircleLine.SetPosition(i, beatCirclePoints[i]);
        }
    }

    public void SetModelColor(Color color)
    {
        if (modelRenderer == null) modelRenderer = model.GetComponent<Renderer>();
        if (modelRenderer.material.color == color) return;
        modelRenderer.material.color = color;
    }

}
