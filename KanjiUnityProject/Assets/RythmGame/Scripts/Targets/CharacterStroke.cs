using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;
using System.Linq;
using System;


// TODO: remove this class as there is only ever one per stroke target + input stroke is doing much at the moment
public class ReferenceStroke
{
    public List<Vector2> keyPointPositions = new List<Vector2>(); // key points in the stroke used for evaluation
    public List<Vector2> points = new List<Vector2>();    // points used for visualising the line on screen
    public float length { get; private set; }
    public Stroke stroke;
    public int nextKeyPointIdx = 0;
    public Vector2? nextKeyPoint { get { return nextKeyPointIdx >= keyPointPositions.Count ? null : keyPointPositions[nextKeyPointIdx]; } }

    public ReferenceStroke(Vector2 scale, Stroke stroke, CharacterStrokeConfig config)
    {
        this.stroke = stroke;
        points.AddRange(stroke.points);
        keyPointPositions = SVGUtils.GetKeyPointsForVectorStroke(stroke.vectorPaths, config.keyPointDistance);
        length = SVGUtils.GetLengthForPnts(points);
        // points are 0 - 1, need to scale up to fit the collider
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = Vector2.Scale(points[i], scale);
        }
        for (int i = 0; i < keyPointPositions.Count; i++)
        {
            keyPointPositions[i] = Vector2.Scale(keyPointPositions[i], scale);
        }
    }

    public void TriggerKeyPoint()
    {
        if (nextKeyPointIdx < keyPointPositions.Count) nextKeyPointIdx++;
    }
}

[Serializable]
public class CharacterStrokeConfig
{
    [Header("Stroke evaluation")]
    public float compThresh = 0.03f;
    [Header("Stroke visuals")]
    public float lineWidth = 2;
    public float keyPointScale = 1.0f;
    public float followTargetScale = 1.0f;
    public float targetZOffset = 0f;
    public float keyPointDistance = 0.02f;
    public float hangaboutTimeCharacter = 0.09f;
}

/// <summary>
/// Goes on prefab representing a singe stroke
/// </summary>
public class CharacterStroke : MonoBehaviour
{
    // draw line
    [SerializeField]
    private LineRenderer referenceStrokeLine;
    [SerializeField]
    private CharacterStrokeFollowTarget followTarget;
    [SerializeField]
    private GameObject keyPointPrefab;

    // effects
    [SerializeField]
    private Effect keyPointCollectEffect;
    [SerializeField]
    private Effect strokePassedEffect;

    // colors
    [SerializeField]
    private Color failColor;
    [SerializeField]
    private Color successColor;
    [SerializeField]
    private Color initialColor;

    // stroke data 
    public ReferenceStroke refStroke = null;
    private CharacterStrokeConfig config;
    private List<SphereCollider> keyPointsColliders = new();

    // results
    public bool Pass { get; set; } = false;
    public bool Completed { get; set; } = false;
    public List<float?> keyPointDeltas = new List<float?>();
    private bool? startBeatHit = null;
    private bool? endBeatHit = null;

    // ref
    private CharacterTarget charTarget;
    private int strokeId;

    // beats
    public BeatManager.Beat StartBeat = null;
    public BeatManager.Beat EndBeat = null;

    // events
    public event Action<CharacterStroke> OnStrokeCompleted;

    public void Init(BeatManager.Beat startBeat, BeatManager.Beat endBeat, Vector2 size, int strokeId, CharacterTarget charTarget, CharacterStrokeConfig config)
    {
        this.config = config;
        this.strokeId = strokeId;
        this.charTarget = charTarget;
        this.StartBeat = startBeat;
        this.EndBeat = endBeat;

        // generate strokes
        refStroke = new ReferenceStroke(size, charTarget.Character.drawData.strokes[strokeId], config);

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
        referenceStrokeLine.startColor = initialColor;
        referenceStrokeLine.endColor = initialColor;


        // add the keypoints
        foreach (Vector2 p in refStroke.keyPointPositions)
        {
            GameObject g = Instantiate(
                keyPointPrefab,
                Vector3.zero,
                Quaternion.identity,
                transform);
            g.transform.localPosition = p;
            g.transform.localScale = g.transform.localScale * config.keyPointScale;
            keyPointsColliders.Add(g.GetComponentInChildren<SphereCollider>());
        }

        // setup the follow circle
        followTarget.transform.localScale = Vector3.Scale(followTarget.transform.localScale, new Vector3(config.followTargetScale, config.followTargetScale, 1));
        followTarget.Init(startBeat, null);
    }

    private void Update()
    {
        // check first beat
        if (!startBeatHit.HasValue && GameInput.GetButton1Down() && followTarget.HitCheck())
        {
            if (GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(followTarget.BeatTimeStamp))
            {
                Instantiate(strokePassedEffect, followTarget.transform.position, Quaternion.identity);
                startBeatHit = true;
            }
            else
            {
                startBeatHit = false;
            }
        }

        // check if on path / collect key points
        if (GameInput.GetButton1())
        {
            Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition());
            if (followTarget.Collider.Raycast(ray, out RaycastHit hit, float.MaxValue))
            {
                followTarget.SetColor(Color.yellow);
                followTarget.RangeCircleActive = true;
                CheckAgainstKeyPoint();
            }
            else
            {
                followTarget.SetColor(Color.white);
                followTarget.RangeCircleActive = false;
            }
        }
        else
        {
            followTarget.SetColor(Color.white);
            followTarget.RangeCircleActive = false;
        }

        // Check the final beat
        if (!endBeatHit.HasValue && GameInput.GetButton1Up() && followTarget.HitCheck())
        {
            if (GameManager.Instance.GameAudio.BeatManager.CheckIfOnBeat(EndBeat.timestamp))
            {
                Instantiate(strokePassedEffect, followTarget.transform.position, Quaternion.identity);
                endBeatHit = true;
            }
            else
            {
                endBeatHit = false;
            }
        }

        UpdateFollowCircle();
    }

    private void UpdateFollowCircle()
    {
        float t = (float)MathUtils.InverseLerp(StartBeat.timestamp, EndBeat.timestamp, AudioSettings.dspTime);
        Vector2 newPos = charTarget.Character.drawData.strokes[strokeId].GetPointOnStroke(t);
        newPos.Scale(charTarget.CharacterSize);
        followTarget.transform.localPosition = newPos;
        if (t >= 1)
        {
            Finish();
        }
    }

    private void Finish()
    {
        Evaluate();
        Completed = true;
        OnStrokeCompleted?.Invoke(this);
    }

    private void Evaluate()
    {
        // HACK: use a struct to capture the key point data correctly
        Pass = startBeatHit.HasValue && startBeatHit.Value == true &&
            keyPointsColliders.Count(c => !c.gameObject.activeInHierarchy) > 1;

        if (Pass)
        {
            referenceStrokeLine.startColor = successColor;
            referenceStrokeLine.endColor = successColor;
        }
        else
        {
            referenceStrokeLine.startColor = failColor;
            referenceStrokeLine.endColor = failColor;
        }
    }

    private void CheckAgainstKeyPoint()
    {
        Collider[] colliders = Physics.OverlapSphere(followTarget.transform.position, followTarget.Collider.radius);
        foreach (SphereCollider s in keyPointsColliders)
        {
            bool canTrigger = s.gameObject.activeInHierarchy && colliders.Contains(s);
            if (canTrigger)
            {
                Instantiate(keyPointCollectEffect, s.transform.position, Quaternion.identity);
                s.gameObject.SetActive(false);
            }

        }
    }


}
