using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;

public class HitTarget : MonoBehaviour
{

    public enum Result 
    {
        Miss,
        Hit
    }

    // text
    [SerializeField]
    private TextMeshPro textMesh;

    // timing
    public double BeatTimeStamp { get; set; } = 0;
    public double StartTimeStamp { get; set; } = 0;
    [SerializeField]
    public double hangAboutTime;

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
    [SerializeField]
    private CapsuleCollider modelCollider;

    // Start is called before the first frame update
    void Start()
    {
        SubscribeToAppEvents();
    }

    public void Init(double beatTimeStamp, string content = null) 
    {
        StartTimeStamp = AudioSettings.dspTime;
        BeatTimeStamp = beatTimeStamp;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; // p
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        radiusEnd = modelCollider.radius;

        if (content != null) textMesh.text = content;
    }

    void Awake()
    {

    }

    void Update()
    {
        bool needsCleanUp = AudioSettings.dspTime >
            BeatTimeStamp + GameManager.Instance.GameAudio.BeatManager.BeatHitAllowance * 1.2f;
        if (needsCleanUp) HandleResult(Result.Miss);

        UpdateBeatCircle();
    }

    public void HandleResult(Result result)
    {
        if (result == Result.Hit) Instantiate(succesEffect, transform.position, Quaternion.identity);
        else if (result == Result.Miss) Instantiate(failEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void UpdateBeatCircle() 
    {
        // decrease size of the beat circle based on time elapsed
        float t = (float) MathUtils.InverseLerp(BeatTimeStamp, StartTimeStamp, AudioSettings.dspTime) ;
        float radius = Mathf.Lerp(radiusEnd, radiusBegin, t);
        GeometryUtils.PopulateCirclePoints3DXY(ref beatCirclePoints, radius, transform.position);
        for (int i = 0; i < beatCirclePoints.Length; i++)
        {
            beatCircleLine.SetPosition(i, beatCirclePoints[i]);
        }
    }




    /// <summary>
    /// On scene closing.
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeToAppEvents();
    }

    /// <summary>
    /// Subscribe to various AppEvents which may trigger or cancel sound effects or music.
    /// </summary>
    private void SubscribeToAppEvents()
    {

    }

    /// <summary>
    /// Unsubscribe to all of the AppEvents which were subscribed to in SubscribeToAppEvents().
    /// </summary>
    private void UnsubscribeToAppEvents()
    {

    }

}
