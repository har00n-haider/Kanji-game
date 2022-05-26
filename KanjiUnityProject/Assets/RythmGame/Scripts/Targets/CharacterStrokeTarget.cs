using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;
using System.Linq;
using System;

public class ReferenceStroke
{
    public List<Vector2> keyPoints = new List<Vector2>(); // key points in the stroke used for evaluation
    public List<Vector2> points = new List<Vector2>();    // points used for visualising the line on screen
    public float length { get; private set; }

    public ReferenceStroke(Vector2 scale, List<Vector2> points, int noOfKeyPoints)
    {
        this.points.AddRange(points);
        // Reference points are 0 - 1, need to scale up to fit the collider
        for (int i = 0; i < this.points.Count; i++)
        {
            this.points[i] = Vector2.Scale(this.points[i], scale);
        }
        keyPoints = SVGUtils.GenRefPntsForPnts(this.points, noOfKeyPoints);
        length = SVGUtils.GetLengthForPnts(this.points);
    }
}

public class InputStroke
{
    // stats/data for the stroke
    public List<Vector2> keyPoints = new(); // key points in the stroke used for evaluation
    public List<Vector2> points = new();    // points used for visualising the line on screen
    public float length { get; private set; }
    public bool completed = false;
    public bool active;
    private int noOfKeyPoints;

    public InputStroke(int noOfKeyPoints)
    {
        this.noOfKeyPoints = noOfKeyPoints;
    }

    public void AddPoint(Vector2 point)
    {
        points.Add(point);
    }

    public void Complete()
    {
        keyPoints = SVGUtils.GenRefPntsForPnts(points, noOfKeyPoints);
        length = SVGUtils.GetLengthForPnts(points);
        completed = true;
        active = false;
    }
}

[Serializable]
public class DrawableStrokeConfig
{
    [Header("Stroke evaluation")]
    // configuration for strokes
    public int noRefPointsInStroke = 5;
    public float compThreshTight = 0.03f;
    public float compThreshLoose = 0.07f;
    public float lengthThreshold = 2;
    [Header("Stroke visuals")]
    public float lineWidth = 2;
    public float targetScale = 1.0f;
    public float targetZOffset = 0f;

}

/// <summary>
/// Goes on prefab representing a singe stroke
/// </summary>
public class CharacterStrokeTarget : MonoBehaviour
{
    // draw line
    private LineRenderer refStrokeLineRenderer;

    // sub targets - tappable targets for the start / end of a given stroke
    [SerializeField]
    private GameObject emptyTargePrefab;
    public EmptyTarget StartTarget { get; private set; } = null;
    public EmptyTarget EndTarget { get; private set; } = null;

    // stroke data 
    public InputStroke inpStroke = null;
    public ReferenceStroke refStroke = null;
    public bool completed { get { return inpStroke.completed; } }
    private DrawableStrokeConfig config;

    // results
    public bool Pass { get; private set; } = true;
    public List<float?> keyPointDeltas = new List<float?>();
    private int tightPointIdx = -1;

    // ref
    private CharacterTarget charTarget;
    private bool canDrawLine = false;

    // beats
    public BeatManager.Beat StarBeat = null;
    public BeatManager.Beat EndBeat = null;

    // state
    public enum StrokeTargetState
    {
        NotStarted,
        InProgress,
    }
    private StrokeTargetState state;



    void Awake()
    {
        config = GameManager.Instance.TargetSpawner.WritingConfig;
    }

    public void Init(BeatManager.Beat startBeat, BeatManager.Beat endBeat, Vector2 size, List<Vector2> points, CharacterTarget charTarget)
    {
        this.charTarget = charTarget;
        this.StarBeat = startBeat;
        this.EndBeat = endBeat;

        // generate strokes
        refStroke = new ReferenceStroke(size, points, config.noRefPointsInStroke);
        inpStroke = new InputStroke(config.noRefPointsInStroke);
        inpStroke.active = false;

        // set the beats for the target, taking care to transform to world pos
        Func<Vector3, Vector3> getWorldPos = (p) =>
        {
            p.z = charTarget.CharacterCenter.z + config.targetZOffset;
            return transform.TransformPoint(p);
        };
        Vector3 startPoint = getWorldPos(refStroke.points.First());
        Vector3 endPosition = getWorldPos(refStroke.points.Last());

        // setup the line renderer to display a line connecting them
        // everything else is set in the component in the editor
        if (refStrokeLineRenderer == null) refStrokeLineRenderer = GetComponentInChildren<LineRenderer>();
        refStrokeLineRenderer.useWorldSpace = false;
        refStrokeLineRenderer.positionCount = refStroke.points.Count;
        refStrokeLineRenderer.SetPositions(refStroke.points.ConvertAll((p) => new Vector3(p.x, p.y, charTarget.CharacterCenter.z)).ToArray());
        refStrokeLineRenderer.startWidth = config.lineWidth;
        refStrokeLineRenderer.endWidth = config.lineWidth;

        // instantiate the start/end targets with their respective beats
        StartTarget = Instantiate(
            emptyTargePrefab,
            startPoint,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        StartTarget.Init(startBeat, null);
        StartTarget.transform.localScale = new Vector3(config.targetScale, config.targetScale, config.targetScale);

        EndTarget = Instantiate(
            emptyTargePrefab,
            endPosition,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        EndTarget.Init(endBeat, null);
        EndTarget.transform.localScale = new Vector3(config.targetScale, config.targetScale, config.targetScale);

        state = StrokeTargetState.NotStarted;
    }

    private void Update()
    {
        bool thresholdPassed = AudioSettings.dspTime >
            EndTarget.BeatTimeStamp + GameManager.Instance.GameAudio.BeatManager.BeatHitAllowance * 1.2f;
        if (thresholdPassed) Finished(Result.Miss);

        switch (state)
        {
            case StrokeTargetState.NotStarted:
                if (GameInput.GetButton1Down())
                {
                    if (StartTarget.HitCheck())
                    {
                        if (GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(StartTarget.BeatTimeStamp))
                        {
                            StartTarget.HandleBeatResult(Result.Hit);
                            state = StrokeTargetState.InProgress;
                            goto case StrokeTargetState.InProgress;
                        }
                        else
                        {
                            StartTarget.HandleBeatResult(Result.Miss);
                            Finished(Result.Miss);
                        }
                    }
                }
                break;
            case StrokeTargetState.InProgress:
                if (GameInput.GetButton1())
                {
                    // convert mouse position to a point on the character plane 
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    bool hit = charTarget.GetCharacterPlane().Raycast(ray, out float enter);
                    if (hit)
                    {
                        // normalize the input points for correct comparison with ref stroke
                        Vector3 worldPoint = ray.direction * enter + ray.origin;
                        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                        inpStroke.AddPoint(localPoint);
                    }
                }
                if (GameInput.GetButton1Up())
                {
                    if (EndTarget.HitCheck())
                    {
                        if (GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(EndTarget.BeatTimeStamp))
                        {
                            EndTarget.HandleBeatResult(Result.Hit);
                            Finished(Result.Hit);
                        }
                        else
                        {
                            EndTarget.HandleBeatResult(Result.Miss);
                            Finished(Result.Miss);
                        }
                    }
                }
                break;
        }
    }

    private void Finished(Result beatResult)
    {
        inpStroke.Complete();
        EvaluateStroke();
        string message = string.Empty;
        if (Pass) message += "Passed stroke | ";
        if (beatResult == Result.Hit) message += "Beat hit";
        Debug.Log(message);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
    }

    public void EvaluateStroke()
    {
        // all points need to be under the loose threshold
        for (int i = 0; i < inpStroke.keyPoints.Count; i++)
        {
            float distance = Mathf.Abs((inpStroke.keyPoints[i] - refStroke.keyPoints[i]).magnitude);
            keyPointDeltas.Add(distance);
            Pass &= distance < config.compThreshLoose;
        }
        // at least one point needs to be under the tight thresh
        float? tightDist = keyPointDeltas.FirstOrDefault(d => d < config.compThreshTight);
        if (tightDist != null) tightPointIdx = keyPointDeltas.IndexOf(tightDist);
        Pass &= tightPointIdx != -1;
        // total length needs to be within limits
        Pass &= Mathf.Abs(inpStroke.length - refStroke.length) < config.lengthThreshold;
    }

}
