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
            this.points[i].Scale(scale);
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
}

/// <summary>
/// Goes on prefab representing a singe stroke
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CharacterStrokeTarget : MonoBehaviour
{

    // draw line
    [SerializeField]
    private LineRenderer refStrokeLineRenderer;

    // sub targets - tappable targets for the start / end of a given stroke
    [SerializeField]
    private GameObject emptyTargePrefab;
    private EmptyTarget startTarget;
    private EmptyTarget endTarget;

    // stroke data 
    public InputStroke inpStroke = null;
    public ReferenceStroke refStroke = null;
    public bool completed { get { return inpStroke.completed; } }
    public DrawableStrokeConfig config;

    // results
    public bool pass = true;
    public List<float?> keyPointDeltas = new List<float?>();
    public int tightPointIdx = -1;




    // Start is called before the first frame update
    void Start()
    {
        AppEvents.OnButtonReleased += InputReleased;
    }

    public void Init(BeatManager.Beat startBeat, BeatManager.Beat endBeat, Vector2 size, List<Vector2> points)  
    {

        // generate strokes
        refStroke = new ReferenceStroke(size, points, config.noRefPointsInStroke);
        inpStroke = new InputStroke(config.noRefPointsInStroke);
        inpStroke.active = false;

        // TODO: hard coded patter for now , replace with character stroke data
        Vector3 startPoint  = points.First();
        Vector3 endPosition = points.Last();

        // setup the line renderer to display a line connecting them
        // everything else is set in the component in the editor
        if (refStrokeLineRenderer == null) refStrokeLineRenderer = GetComponent<LineRenderer>();
        refStrokeLineRenderer.useWorldSpace = true;
        refStrokeLineRenderer.positionCount = points.Count;
        refStrokeLineRenderer.SetPositions( points.ConvertAll( (p) => new Vector3(p.x, p.y , 0)).ToArray()); //TODO: get the z value from the character? 

        // instantiate the start/end targets with their respective beats
        startTarget = Instantiate(
            emptyTargePrefab,
            startPoint,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        startTarget.Init(startBeat, null);
        startTarget.OnHitSuccesfully += StartLoggingInput;

        endTarget = Instantiate(
            emptyTargePrefab,
            endPosition,
            Quaternion.identity,
            transform).GetComponent<EmptyTarget>();
        endTarget.Init(endBeat, null);
        endTarget.OnHitSuccesfully += StopLoggingInput;


    }

    void Awake()
    {

    }

    void Update()
    {
    }


    private void OnDestroy()
    {
        AppEvents.OnButtonReleased -= InputReleased;
    }

    public void HandleBeatResult(Result result)
    {
        throw new System.NotImplementedException();
    }

    private void InputReleased()
    {
    
    }

    private void StartLoggingInput()
    {
    
    }

    private void StopLoggingInput()
    {
    
    }

    public void EvaluateStroke()
    {
        // all points need to be under the loose threshold
        for (int i = 0; i < inpStroke.keyPoints.Count; i++)
        {
            float distance = Mathf.Abs((inpStroke.keyPoints[i] - refStroke.keyPoints[i]).magnitude);
            keyPointDeltas.Add(distance);
            pass &= distance < config.compThreshLoose;
        }
        // at least one point needs to be under the tight thresh
        float? tightDist = keyPointDeltas.FirstOrDefault(d => d < config.compThreshTight);
        if (tightDist != null) tightPointIdx = keyPointDeltas.IndexOf(tightDist);
        pass &= tightPointIdx != -1;
        // total length needs to be within limits
        pass &= Mathf.Abs(inpStroke.length - refStroke.length) < config.lengthThreshold;
    }

}
