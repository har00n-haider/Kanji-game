using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class GameManager : MonoBehaviour
{
    public static GameManager instance; 

    public GameAudio gameAudio;

    public UnityEngine.UI.Extensions.UICircle circle;
    public UnityEngine.UI.Extensions.UICircle circle1;

    [SerializeField]
    private Color beatFlickercColor;
    private bool colorToggle = false;

    // Awake() is called before Start.
    void Awake()
    {
        SubscribeToAppEvents();
    }

    // Start is called before the first frame update
    void Start()
    {
        AppEvents.OnStartLevel?.Invoke();

        if (instance == null) instance = this;
   
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

        UpdateColor();
    }


    public void ToggleColor()
    {
        colorToggle = !colorToggle;

        if (colorToggle)
        {
            circle.color = Color.red;
        }
        else 
        {
            circle.color = Color.black;
        }
    }

    public void UpdateColor()
    {
        if (gameAudio.SongManager.CheckIfOnBeat())
        {
            circle1.color = Color.red;
        }
        else
        {
            circle1.color = Color.black;
        }
    }



    public void BeatHit() 
    {
        if (gameAudio.SongManager.CheckIfOnBeat()) 
        {
            Debug.Log("hit");
        }
        else
        {
            Debug.Log("miss");
        }
    }

    void UpdateClick() 
    {
        //if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        //{
        //    RaycastHit hit;
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        Transform objectHit = hit.transform;
        //        // Debug.Log($"GameManager.Update(): objectHit = {objectHit.name}, tag = {objectHit.tag}");

        //        // Check the hit detect is a Firework - just use Unity tags for this, simple.
        //        bool isFirework = objectHit.CompareTag("Firework");
        //        bool onBeat = false; // TODO: fixme
        //        if (isFirework && onBeat)
        //        {
        //            GameObject firework = objectHit.gameObject;
        //            // Instantiate an Explosion at current position of the firework and zero rotation.
        //            GameObject spawnedExplosion = Instantiate(_explosionPrefab, firework.transform.position, Quaternion.identity);
        //            AppEvents.OnSpawnExplosion?.Invoke(spawnedExplosion);
        //            AppEvents.OnBeatClick?.Invoke(firework);

        //            // Kill the firework.
        //            Destroy(firework);           // <---- Destroy()-ing the GameObject causes trouble, see workaround above.
        //            Destroy(spawnedExplosion, 3.0f); // HACK: destroy after effect plays out
        //        }
        //        else if (isFirework && !onBeat)
        //        {
        //            GameObject firework = objectHit.gameObject;
        //            AppEvents.OnFireworkMissed?.Invoke(firework);
        //            // Instantiate an Explosion at current position of the firework and zero rotation.
        //            GameObject spawnedFizzle = Instantiate(_fizzlePrefab, firework.transform.position, Quaternion.identity);
        //            Destroy(firework);
        //            Destroy(spawnedFizzle, 3.0f); // HACK: destroy after effect plays out
        //        }
        //        else
        //        {
        //            Debug.Log("Hitting: " + objectHit.name);
        //        }
        //    }
        //}

    }
}
