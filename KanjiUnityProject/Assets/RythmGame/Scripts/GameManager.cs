using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Manabu.Core;
using RythmGame;

/// <summary>
/// - Singleton pattern
/// - Deals with input in the game
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameAudio GameAudio;
    public Database Database;
    public TextAsset databaseFile;

    //TODO: delete me - debug
    public Image circle;

    [SerializeField]
    private Color BarFlickercColor;
    [SerializeField]
    private Color BeatFlickercColor;

    // Awake() is called before Start.
    void Awake()
    {
        Database = new Database();
        Database.Load(databaseFile);
        if (Instance == null) Instance = this;
        SubscribeToAppEvents();
    }

    // Start is called before the first frame update
    void Start()
    {
        AppEvents.OnStartLevel?.Invoke();
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
        CheckForHitTargetClicked();
        UpdateBeat();
    }

    // TODO: delete me
    public void UpdateBeat()
    {
        BeatManager.Beat beat = GameAudio.BeatManager.NextBeat;
        bool onBeat = GameAudio.BeatManager.CheckIfOnBeat(beat.timestamp);
        if (onBeat && beat.type == BeatManager.Beat.BeatType.Bar)
        {
            circle.color = BarFlickercColor;
        }
        else if (onBeat && beat.type == BeatManager.Beat.BeatType.Beat)
        {
            circle.color = BeatFlickercColor;
        }
        else
        {
            circle.color = Color.black;
        }
    }

    void CheckForHitTargetClicked()
    {
        bool buttonPressed =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space);
        if (buttonPressed) AppEvents.OnButtonPressed?.Invoke();

        bool buttonReleased =
            Input.GetMouseButtonUp(0) ||
            Input.GetKeyUp(KeyCode.Space);
        if (buttonReleased) AppEvents.OnButtonReleased?.Invoke();

        // TODO: do we want to do both button down and up globally?
        // We are doing this for draw targets aswell but it may be easier to tap targets
        // if we are runnig the check two frames per button press? May not be a big deal tho.
        bool runRayCastCheck = buttonPressed || buttonReleased;
        
        if (runRayCastCheck)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                // Check the hit detect is a target - just use Unity tags for this, simple.
                bool isTarget = hit.transform.gameObject.CompareTag("HitTarget");
                if (!isTarget) return; 
                ITappable ht = hit.transform.parent.gameObject.GetComponent<ITappable>();
                if (ht != null)
                {
                    bool onBeat = GameAudio.BeatManager.CheckIfOnBeat(ht.BeatTimeStamp);
                    if (onBeat)
                    {
                        ht.HandleBeatResult(Result.Hit);
                    }
                    else
                    {
                        ht.HandleBeatResult(Result.Miss);
                    }
                    return;
                }
            }
        }
    }
}
