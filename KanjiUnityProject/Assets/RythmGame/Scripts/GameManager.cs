using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using KanjiLib.Core;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameAudio GameAudio;
    public KanjiDatabase KanjiDatabase;

    //TODO: delete me - debug
    public UnityEngine.UI.Extensions.UICircle circle;

    [SerializeField]
    private Color BarFlickercColor;
    [SerializeField]
    private Color BeatFlickercColor;

    // Awake() is called before Start.
    void Awake()
    {
        KanjiDatabase = new KanjiDatabase();
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
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                // Check the hit detect is a target - just use Unity tags for this, simple.
                bool isTarget = hit.transform.gameObject.CompareTag("HitTarget");
                if (!isTarget) return; 
                HitTarget ht = hit.transform.parent.gameObject.GetComponent<HitTarget>();
                if (ht != null)
                {
                    bool onBeat = GameAudio.BeatManager.CheckIfOnBeat(ht.BeatTimeStamp, false);
                    if (onBeat)
                    {
                        ht.HandleBeatResult(HitTarget.Result.Hit);
                    }
                    else
                    {
                        ht.HandleBeatResult(HitTarget.Result.Miss);
                    }
                    return;
                }
            }
        }
    }
}
