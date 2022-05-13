using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;

public class HitTarget : MonoBehaviour
{

    [SerializeField]
    private UnityEngine.UI.Button button;

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

        originalColor = button.colors.normalColor;
    }

    void Awake()
    {
        //HACK: yes this is disgusting, but no time...
        gameAudio = GameObject.FindWithTag("GameAudio").GetComponent<GameAudio>();
        button = GetComponent<UnityEngine.UI.Button>();

        Assert.IsNotNull(button);
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

    void Update()
    {

    }

    public void Explode() 
    {
        if (isExploded) return;

        //TODO: fixme
        //trailEffect.SetInt(SPAWN_RATE_NAME, 0); // turns off trail
        isExploded = true;
    }


}
