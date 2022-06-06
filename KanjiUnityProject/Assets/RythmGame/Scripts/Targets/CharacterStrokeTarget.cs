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
    public bool Completed = false;
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
        Completed = true;
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
    public float followCircleScale = 1.0f;
    public float targetZOffset = 0f;


}

/// <summary>
/// Goes on prefab representing a singe stroke
/// </summary>
public class CharacterStrokeTarget : MonoBehaviour
{
    // draw line
    [SerializeField]
    private LineRenderer referenceStrokeLine;
    [SerializeField]
    private GameObject followCircle;

    

    // sub targets - tappable targets for the start / end of a given stroke
    [SerializeField]
    private GameObject emptyTargePrefab;
    public EmptyTarget StartTarget { get; private set; } = null;

    // stroke data 
    public InputStroke inpStroke = null;
    public ReferenceStroke refStroke = null;
    private DrawableStrokeConfig config;

    // results
    public bool Completed { get { return inpStroke.Completed; } }
    public bool Pass { get { return startBeatHit && endBeatHit && strokePassedEvaluation; } }
    public List<float?> keyPointDeltas = new List<float?>();
    private int tightPointIdx = -1;
    private bool startBeatHit = false; 
    private bool endBeatHit = false; 
    private bool strokePassedEvaluation = true; 

    // ref
    private CharacterTarget charTarget;
    private int strokeId;

    // beats
    public BeatManager.Beat StartBeat = null;
    public BeatManager.Beat EndBeat = null;

    // state
    private enum StrokeTargetState
    {
        NotStarted,
        InProgress,
        Finished
    }
    private StrokeTargetState state;

    // events
    public event Action<CharacterStrokeTarget> OnStrokeCompleted;

    void Awake()
    {
        config = GameManager.Instance.TargetSpawner.WritingConfig;
    }

    public void Init(BeatManager.Beat startBeat, BeatManager.Beat endBeat, Vector2 size, int strokeId, CharacterTarget charTarget)
    {
        this.strokeId = strokeId;
        this.charTarget = charTarget;
        this.StartBeat = startBeat;
        this.EndBeat = endBeat;

        // generate strokes
        refStroke = new ReferenceStroke(size, charTarget.Character.drawData.strokes[strokeId].points, config.noRefPointsInStroke);
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
        referenceStrokeLine.useWorldSpace = false;
        referenceStrokeLine.positionCount = refStroke.points.Count;
        referenceStrokeLine.SetPositions(refStroke.points.ConvertAll((p) => new Vector3(p.x, p.y, charTarget.CharacterCenter.z)).ToArray());
        referenceStrokeLine.startWidth = config.lineWidth;
        referenceStrokeLine.endWidth = config.lineWidth;

        // setup the follow circle
        followCircle.transform.localScale = Vector3.Scale(followCircle.transform.localScale, new Vector3(config.followCircleScale, config.followCircleScale, 1));

        // instantiate the start/end targets with their respective beats
        StartTarget = Instantiate(
            emptyTargePrefab,
            startPoint,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        StartTarget.Init(startBeat, null);
        StartTarget.transform.localScale = new Vector3(config.targetScale, config.targetScale, config.targetScale);

        state = StrokeTargetState.NotStarted;
    }

    private void Update()
    {
        if (state == StrokeTargetState.Finished) return;

        bool thresholdPassed = AudioSettings.dspTime >
            EndBeat.timestamp + GameManager.Instance.GameAudio.BeatManager.BeatHitAllowance * 1.2f;
        if (thresholdPassed) Finish();

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
                            startBeatHit = true;
                            goto case StrokeTargetState.InProgress;
                        }
                        else
                        {
                            StartTarget.HandleBeatResult(Result.Miss);
                            startBeatHit = false;
                            Finish();
                        }
                    }
                }
                break;
            case StrokeTargetState.InProgress:
                state = StrokeTargetState.InProgress;
                // Update the input stroke 
                if (GameInput.GetButton1())
                {
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
                // Check the final beat
                if (GameInput.GetButton1Up())
                {
                    if (GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(EndBeat.timestamp))
                    {
                        endBeatHit = true;
                        Finish();
                    }
                    else
                    {
                        endBeatHit = false;
                        Finish();
                    }
                }
                break;
        }

        UpdateFollowCircle();
    }


    private void OnDestroy()
    {
    }

    private void UpdateFollowCircle()
    {
        float t = (float)MathUtils.InverseLerp(StartBeat.timestamp, EndBeat.timestamp, AudioSettings.dspTime);
        Vector2 newPos = charTarget.Character.drawData.strokes[strokeId].GetPointOnStroke(t);
        newPos.Scale(charTarget.CharacterSize);
        Debug.Log(t + " " + newPos);
        followCircle.transform.localPosition = newPos;

    }

    private void Finish()
    {
        state = StrokeTargetState.Finished;
        inpStroke.Complete();
        EvaluateInputStroke();
        //Debug.Log("Passed stroke "  + Pass + " | start beat hit " + startBeatHit + "| end beat hit" + endBeatHit);
        OnStrokeCompleted?.Invoke(this);
    }

    public void EvaluateInputStroke()
    {
        // all points need to be under the loose threshold
        for (int i = 0; i < inpStroke.keyPoints.Count; i++)
        {
            float distance = Mathf.Abs((inpStroke.keyPoints[i] - refStroke.keyPoints[i]).magnitude);
            keyPointDeltas.Add(distance);
            strokePassedEvaluation &= distance < config.compThreshLoose;
        }
        // at least one point needs to be under the tight thresh
        float? tightDist = keyPointDeltas.FirstOrDefault(d => d < config.compThreshTight);
        if (tightDist != null) tightPointIdx = keyPointDeltas.IndexOf(tightDist);
        strokePassedEvaluation &= tightPointIdx != -1;
        // total length needs to be within limits
        strokePassedEvaluation &= Mathf.Abs(inpStroke.length - refStroke.length) < config.lengthThreshold;
    }

}
