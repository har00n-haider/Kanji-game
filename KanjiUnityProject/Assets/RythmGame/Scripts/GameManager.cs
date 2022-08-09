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
    public BeatManager BeatManager;



    // configuration
    [Header("----Game settings----")]
    [SerializeField]
    private TextAsset databaseFile;
    public GameSettings Settings;

    [Header("----Debug----")]
    public Image circle; // circle for checking the beat timing is correct
    [SerializeField]
    private Color BeatFlickercColor;

    [Header("----song/beatmap to load----")]
    public string beatmapPath;

    // Awake() is called before Start.
    void Awake()
    {
        Database = new Database();
        Database.Load(databaseFile);
        if (Instance == null) Instance = this;
        SubscribeToAppEvents();

        // load in the first song
        BeatMapData beatmapData = OsuBeatMapParser.Parse(beatmapPath);
        BeatManager.Init(beatmapData);
        TargetSpawner.Init(beatmapData);
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
        if(BeatManager.CheckIfOnSongBeat())
        {
            circle.color = BeatFlickercColor;
        }
        else
        {
            circle.color = Color.black;
        }
    }

}
