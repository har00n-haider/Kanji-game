using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;
using System.Linq;


public class StrokeTarget : MonoBehaviour, ITappable
{

    // draw line
    [SerializeField]
    private LineRenderer inputLineRenderer;

    // data
    private Character character;
    private int strokeNumber;


    // sub targets
    [SerializeField]
    private GameObject emptyTargePrefab;
    private EmptyTarget startTarget;
    private EmptyTarget endTarget;

    public double BeatTimeStamp => throw new System.NotImplementedException();

    // Start is called before the first frame update
    void Start()
    {
        AppEvents.OnButtonReleased += InputReleased;
    }

    // Takes 
    public void Init(BeatManager.Beat startBeat, BeatManager.Beat endBeat, Character character, int strokeId)  
    {

        this.character = character;
        // TODO: hard coded patter for now , replace with character stroke data
        Vector3 startPoint  = new Vector3(3*strokeId,7,5);
        Vector3 endPosition = new Vector3(3*strokeId,0,5);
        Vector3[] linePoints = new Vector3[20];
        GeometryUtils.PopulateLinePoints(ref linePoints, startPoint, endPosition);
        
        // setup the line renderer to display a line connecting them
        // everything else is set in the component in the editor
        inputLineRenderer.useWorldSpace = true;
        inputLineRenderer.positionCount = linePoints.Length;
        inputLineRenderer.SetPositions(linePoints);

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
        endTarget.OnHitSuccesfully += StartLoggingInput;



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


}
