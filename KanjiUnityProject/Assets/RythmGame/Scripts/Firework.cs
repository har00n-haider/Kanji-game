using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;

public class Firework : MonoBehaviour
{
    // This sound asset is played on collision.
    private AudioSource _audioSourceFirework;
    public float elapsedTime = 0.0f;
    public float clickTime = 2.0f;

    public bool missedClick = false;

    // firework effect stuff
    public VisualEffect trailEffect;
    public static readonly string SPAWN_RATE_NAME = "trail spawn rate";
    public bool isExploded = false;
    private Rigidbody rb;
    private Renderer[] renderers;
    private Color originalColor;
    public Color beatFlickercColor;

    private GameAudio gameAudio;

    // Start is called before the first frame update
    void Start()
    {
        SubscribeToAppEvents();
        trailEffect.SendEvent("OnPlay");

        _audioSourceFirework = GetComponent<AudioSource>();
        Assert.IsNotNull(_audioSourceFirework);
        
        rb = GetComponent<Rigidbody>();


    }

    void Awake()
    {
        
        //HACK: yes this is disgusting, but no time...
        gameAudio = GameObject.FindWithTag("GameAudio").GetComponent<GameAudio>();
        renderers = GetComponentsInChildren<Renderer>();
        originalColor = renderers[0].material.color;
    }

    /// <summary>
    /// On scene closing.
    /// </summary>
    private void OnDestroy()
    {
        checkClickTime();
        UnsubscribeToAppEvents();
    }

    /// <summary>
    /// Subscribe to various AppEvents which may trigger or cancel sound effects or music.
    /// </summary>
    private void SubscribeToAppEvents()
    {
        AppEvents.OnSpawnFirework += HandleSpawnFirework;
        AppEvents.OnFireworkMissed += HandleFireworkMissed;
    }

    /// <summary>
    /// Unsubscribe to all of the AppEvents which were subscribed to in SubscribeToAppEvents().
    /// </summary>
    private void UnsubscribeToAppEvents()
    {
        AppEvents.OnSpawnFirework -= HandleSpawnFirework;
        AppEvents.OnFireworkMissed -= HandleFireworkMissed;
    }

    void Update()
    {
        trackSpawnTime();
        UpdateBeatFlicker();
    }

    void UpdateBeatFlicker() 
    {
        if (gameAudio.CheckIfOnBeat()) 
        {
            foreach (Renderer r in renderers)
            {
                r.material.color = originalColor;
            }
        }
        else 
        {
            foreach (Renderer r in renderers)
            {
                r.material.color = beatFlickercColor;
            }
        }
    }


    private void checkClickTime(){
        // Debug.Log($"checkClickTime(), elapsedTime: {elapsedTime}");
        if(elapsedTime >= clickTime){
            missedClick = true;
            AppEvents.OnFireworkMissed?.Invoke(this.gameObject);
        }
    }

    private void trackSpawnTime(){
        //Debug.Log("trackSpawnTime()");
        elapsedTime += Time.deltaTime;
        //Debug.Log($"elapsedTime: {elapsedTime}");
    }

    private void HandleSpawnFirework(GameObject firework, Vector3 launchForce)
    {
        //Debug.Log("HandleSpawnFirework");
    }


    public void Explode() 
    {
        if (isExploded) return;

        trailEffect.SetInt(SPAWN_RATE_NAME, 0); // turns off trail
        isExploded = true;
    }
    private void HandleFireworkMissed(GameObject firework)
    {
        //Debug.Log("HandleFireworkMissed");
    }
}
