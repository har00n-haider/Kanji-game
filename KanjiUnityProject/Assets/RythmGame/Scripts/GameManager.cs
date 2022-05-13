using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private GameObject _fizzlePrefab;

    public GameAudio gameAudio;

    // Awake() is called before Start.
    void Awake()
    {
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

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                // Debug.Log($"GameManager.Update(): objectHit = {objectHit.name}, tag = {objectHit.tag}");

                // Check the hit detect is a Firework - just use Unity tags for this, simple.
                bool isFirework = objectHit.CompareTag("Firework");
                bool onBeat = gameAudio.CheckIfOnBeat();
                if (isFirework && onBeat)
                {
                    GameObject firework = objectHit.gameObject;
                    // Instantiate an Explosion at current position of the firework and zero rotation.
                    GameObject spawnedExplosion = Instantiate(_explosionPrefab, firework.transform.position, Quaternion.identity);
                    AppEvents.OnSpawnExplosion?.Invoke(spawnedExplosion);
                    AppEvents.OnFireworkClickHit?.Invoke(firework);

                    // Kill the firework.
                    Destroy(firework);           // <---- Destroy()-ing the GameObject causes trouble, see workaround above.
                    Destroy(spawnedExplosion, 3.0f); // HACK: destroy after effect plays out
                }
                else if (isFirework && !onBeat) 
                {
                    GameObject firework = objectHit.gameObject;
                    AppEvents.OnFireworkMissed?.Invoke(firework);
                    // Instantiate an Explosion at current position of the firework and zero rotation.
                    GameObject spawnedFizzle = Instantiate(_fizzlePrefab, firework.transform.position, Quaternion.identity);
                    Destroy(firework);
                    Destroy(spawnedFizzle, 3.0f); // HACK: destroy after effect plays out
                }
                else 
                {
                    Debug.Log("Hitting: " + objectHit.name);
                }
            }
        }

    }
}
