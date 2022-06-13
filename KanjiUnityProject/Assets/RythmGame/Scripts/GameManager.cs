using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Manabu.Core;


/// <summary>
/// Singleton pattern managment object for the game
/// </summary>
public class GameManager : MonoBehaviour
{
    // refs
    public static GameManager Instance;
    public GameAudio GameAudio;
    public Database Database;
    public GameInput GameInput;
    public TargetSpawner TargetSpawner;


    // configuration
    [Header("----Game settings----")]
    [SerializeField]
    private TextAsset databaseFile;
    public GameSettings Settings;

    [Header("----Debug----")]
    public Image circle; // circle for checking the beat timing is correct
    [SerializeField]
    private Color BarFlickercColor;
    [SerializeField]
    private Color BeatFlickercColor;
    [SerializeField]
    private Color HalfBeatFlickercColor;

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
        UpdateBeat();
    }

    public void UpdateBeat()
    {
        Beat beat = GameAudio.BeatManager.NextBeat;
        bool onBeat = GameAudio.BeatManager.CheckIfOnBeat(beat);
        if(onBeat)
        {
            switch (beat.type)
            {
                case Beat.BeatType.HalfBeat:
                    circle.color = HalfBeatFlickercColor;
                    break;
                case Beat.BeatType.Beat:
                    circle.color = BeatFlickercColor;
                    break;
                case Beat.BeatType.Bar:
                    circle.color = BarFlickercColor;
                    break;
            }
        }
        else
        {
            circle.color = Color.black;
        }
    }

}
