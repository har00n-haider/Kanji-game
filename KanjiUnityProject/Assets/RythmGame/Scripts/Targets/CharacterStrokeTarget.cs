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
        keyPoints = SVGUtils.GenRefPntsForPnts(this.points, noOfKeyPoints);
        length = SVGUtils.GetLengthForPnts(this.points);
        // Reference points are 0 - 1, need to scale up to fit the collider
        for (int i = 0; i < this.points.Count; i++)
        {
            this.points[i] = Vector2.Scale(this.points[i], scale);
        }
    }
}

public class InputStroke
{
    // stats/data for the stroke
    public List<Vector2> keyPoints = new(); // key points in the stroke used for evaluation
    public List<Vector2> points = new();    // points used for visualising the line on screen
    public float length { get; private set; }
    public bool completed;
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
}

/// <summary>
/// Goes on prefab representing a singe stroke
/// </summary>
[RequireComponent(typeof(LineRenderer))]
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

        // set the beats for the target
        Vector3 startPoint  = refStroke.points.First();
        Vector3 endPosition = refStroke.points.Last();

        // setup the line renderer to display a line connecting them
        // everything else is set in the component in the editor
        if (refStrokeLineRenderer == null) refStrokeLineRenderer = GetComponent<LineRenderer>();
        //refStrokeLineRenderer.useWorldSpace = false;
        refStrokeLineRenderer.positionCount = refStroke.points.Count;
        refStrokeLineRenderer.SetPositions( refStroke.points.ConvertAll( (p) => new Vector3(p.x, p.y , charTarget.CharacterCenter.z)).ToArray()); 
        refStrokeLineRenderer.startWidth = config.lineWidth; 
        refStrokeLineRenderer.endWidth= config.lineWidth; 

        // instantiate the start/end targets with their respective beats
        StartTarget = Instantiate(
            emptyTargePrefab,
            startPoint,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        StartTarget.Init(startBeat, null);
        StartTarget.OnHitSuccesfully += StartLoggingInput;

        EndTarget = Instantiate(
            emptyTargePrefab,
            endPosition,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        EndTarget.Init(endBeat, null);
        EndTarget.OnHitSuccesfully += StopLoggingInput;
    }

    private void Update()
    {
        if(canDrawLine)
        {        
            // populate line
            if (GameManager.Instance.GameInput.GetButton1Down())
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
            // clear line
            if (GameManager.Instance.GameInput.GetButton1Up())
            {
                inpStroke.Complete();
                EvaluateStroke();
            }
        }
    }


    private void OnDestroy()
    {
    }

    private void StartLoggingInput()
    {
        canDrawLine = true;
    }

    private void StopLoggingInput()
    {
        canDrawLine = false;
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
