using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;

public class HitTarget : MonoBehaviour
{
    public double BeatTimeStamp { get; set; } = 0;
    public double StartTimeStamp { get; set; } = 0;

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


    // This sound asset is played on collision.
    private AudioSource _audioSourceFirework;

    // effect stuff
    private VisualEffect trailEffect;
    private static readonly string SPAWN_RATE_NAME = "trail spawn rate";
    private bool isExploded = false;
    private Color originalColor;

    private GameAudio gameAudio;

    // Start is called before the first frame update
    void Start()
    {
        SubscribeToAppEvents();

        // TODO: FIX Me 
        //trailEffect.SendEvent("OnPlay");

        //_audioSourceFirework = GetComponent<AudioSource>();
        //Assert.IsNotNull(_audioSourceFirework);

    }

    public void Init(double beatTimeStamp) 
    {
        StartTimeStamp = AudioSettings.dspTime;
        BeatTimeStamp = beatTimeStamp;
        beatCircleLine.positionCount = beatCirclePoints.Length;
        beatCircleLine.useWorldSpace = true;
        beatCircleLine.numCapVertices = 10; // p
        beatCircleLine.endWidth = beatCircleLineWidth;  
        beatCircleLine.startWidth = beatCircleLineWidth;
        radiusEnd = modelCollider.radius;
    }

    void Awake()
    {
        //HACK: yes this is disgusting, but no time...
        gameAudio = GameObject.FindWithTag("GameAudio").GetComponent<GameAudio>();

    }

    
    void Update()
    {
        UpdateBeatCircle();
    }

    private void UpdateBeatCircle() 
    {
        float t = (float) MathUtils.InverseLerp(BeatTimeStamp, StartTimeStamp, AudioSettings.dspTime) ;
        float radius = Mathf.Lerp(radiusEnd, radiusBegin, t);
        GeometryUtils.PopulateCirclePoints3DXY(ref beatCirclePoints, radius, transform.position);
        for (int i = 0; i < beatCirclePoints.Length; i++)
        {
            beatCircleLine.SetPosition(i, beatCirclePoints[i]);
        }
    }

    public void Explode() 
    {
        if (isExploded) return;

        //TODO: fixme
        //trailEffect.SetInt(SPAWN_RATE_NAME, 0); // turns off trail
        isExploded = true;
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
