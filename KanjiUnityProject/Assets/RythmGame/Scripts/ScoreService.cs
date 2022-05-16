using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreService : MonoBehaviour
{

    /// <summary>
    /// Holds the current score
    /// </summary>
    public float currentScore;

    /// <summary>
    /// Holds the current multiplier
    /// </summary>
    public float currentMultiplier;

    /// <summary>
    /// Holds the current multiplier
    /// </summary>
    public float totalClicked = 0;

    /// <summary>
    /// Holds most recent earned points
    /// </summary>
    public float newPoints;

    /// <summary>
    /// Holds most recent multiplier modification value
    /// </summary>
    public float multiplierMod;


    /// <summary>
    /// Minimum possible score
    /// </summary>
    public const float minMultiplier = 0.0f;

    /// <summary>
    /// Maximum possible score
    /// </summary>
    public const float maxMultiplier = 16.0f;

    //public float smallFirewokBonus = 0.25f;
    //public float mediumFirewokBonus = 0.50f;
    //public float largeFirewokBonus = 0.75f;

    /// <summary>
    /// Earned multiplier from successful tap
    /// </summary>
    public float firewokMultiplier = 0.50f;

    /// <summary>
    /// The amount the earned multiplier is increased, to decrement from missed tap
    /// </summary>
    public float missedMultiplierDecrement = 2.0f;

    /// <summary>
    /// Dict with buckets/corresponding scores
    /// </summary>
    public Dictionary<float, int> scoreRanges = new Dictionary<float, int>();

    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text multiplierText;
    public TMPro.TMP_Text multiplierModText;
    public TMPro.TMP_Text scoreModText;


    // Awake() is called before Start.
    void Awake()
    {
        SubscribeToAppEvents();
        /// <summary>
        /// Ugly, I know
        /// </summary>
        scoreRanges.Add(0.0f, 2);
        scoreRanges.Add(0.1f, 4);
        scoreRanges.Add(0.2f, 6);
        scoreRanges.Add(0.3f, 8);
        scoreRanges.Add(0.4f, 12);
        scoreRanges.Add(0.5f, 25);
        scoreRanges.Add(0.6f, 50);
        scoreRanges.Add(0.7f, 100);
        scoreRanges.Add(0.8f, 120);
        scoreRanges.Add(0.9f, 200);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void FixedUpdate()
    {
        //var foundFireworkObjects = FindObjectsOfType<Firework>();
        //Debug.Log(foundFireworkObjects + " : " + foundFireworkObjects.Length);
        scoreText.text = $"{currentScore} Points";
        multiplierText.text = $"{currentMultiplier}X Multiplier";
    }
        
    private IEnumerator DisplayIncremenetCountdown(string modType, float newPoints)
    {
        float duration = 1.2f; 
        float normalizedTime = 0;
        Color greenColor = new Color(0.0f, 255.0f, 0.0f, 1.0f);
        Color redColor = new Color(255.0f, 0.0f, 0.0f, 1.0f);

        while(normalizedTime <= 1f)
        {
            normalizedTime += Time.deltaTime / duration;

            if(modType == "score")
            {
                scoreModText.gameObject.SetActive(true);
                
                scoreModText.faceColor = greenColor;
                scoreModText.text = "";
                scoreModText.text = $"{newPoints}";
            }
            else
            {
                float tmp_mmod = multiplierMod * -1.0f;
                multiplierModText.gameObject.SetActive(true);
                if(tmp_mmod > 0){
                    multiplierModText.faceColor = greenColor;
                    multiplierModText.text = "";
                    multiplierModText.text = $"+{tmp_mmod}";
                }
                if(tmp_mmod < 0){
                    
                    
                    multiplierModText.faceColor = redColor;
                    multiplierModText.text = "";
                    multiplierModText.text = $"-{tmp_mmod}";
                }
            }
            yield return null;
        }
        if(modType == "score")
        {
            scoreModText.faceColor = Color.Lerp(greenColor, new Color(greenColor.r, greenColor.g, greenColor.b, 0.0f), 0.05f * Time.deltaTime);
            scoreModText.gameObject.SetActive(false);
        }
        else 
        {
            multiplierModText.gameObject.SetActive(false);
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
        AppEvents.OnStartLevel += HandleStartLevel;
        AppEvents.OnBeatHit += HandleFireworkClickHit;
        AppEvents.OnBeatMissed += HandleFireworkClickMissed;
    }

    /// <summary>
    /// Unsubscribe to all of the AppEvents which were subscribed to in SubscribeToAppEvents().
    /// </summary>
    private void UnsubscribeToAppEvents()
    {
        AppEvents.OnStartLevel -= HandleStartLevel;
        AppEvents.OnBeatHit += HandleFireworkClickHit;
        AppEvents.OnBeatMissed += HandleFireworkClickMissed;
    }

    //AppEvents.OnStartLevel += HandleStartLevel;

    private void HandleStartLevel()
    {
        // PJH TODO - remove log
        // Debug.Log("ScoreService.HandleStartLevel()");
    }

    private void HandleSpawnFirework(GameObject fireworkInstance)
    {
        // Get the AudioSource from the instance of the GameObject
    }
    private void HandleFireworkClickHit(GameObject fireworkInstance)
    {
        //// Get the AudioSource from the instance of the GameObject
        //float eTime = fireworkInstance.GetComponent<HitTarget>().elapsedTime;
        //float cTime = fireworkInstance.GetComponent<HitTarget>().clickTime;
        ////Debug.Log("HandleFireworkClickHit");
        //if(eTime < cTime){
        //    float increment = eTime/cTime;
        //    //Debug.Log($"HandleFireworkClickHit.increment: {increment}");
        //    incrementScore(increment);
        //    totalClicked +=1;
        //}
        
    }
    private void HandleFireworkClickMissed(GameObject fireworkInstance)
    {
        // Get the AudioSource from the instance of the GameObject
        decrementMultiplier();
        //Debug.Log("decrementMultiplier()");
    }
    /// <summary>
    /// Takes float value and limits to passed min/max values
    /// </summary>
    // public float LimitToRange(this float value, float inclusiveMinimum, float inclusiveMaximum)
    // {
    //     if (value < inclusiveMinimum) { return inclusiveMinimum; }
    //     if (value > inclusiveMaximum) { return inclusiveMaximum; }
    //     return value;
    // }

    /// <summary>
    /// Coerces anything above 0.9 to 200 points, grabs correct score value from scoreRanges dict
    /// </summary>
    private float calcualteScore(float timeBucket)
    {
        // Takes the rounded normalised score and fetches the corresponding point value from the scoreRanges dict

        float collectedScore;
        float timeKey = Mathf.Clamp((float)timeBucket, 0.0f, 0.9f);
        //Debug.Log("calcualteScore()");
        try
        {
            //timeKey = LimitToRange((float)timeBucket, 0.0f, 0.9f);

            if(scoreRanges.ContainsKey(timeKey))
            {
                collectedScore = scoreRanges[timeKey];
                //Debug.Log($"collectedScore: {collectedScore}");
                return collectedScore * currentMultiplier;
            }
        } 
        catch (Exception e) 
        {
            Debug.Log($"Score out of range 0.0-1.0, sent score: {timeKey}");
            Debug.Log($"Exception: {e}");
        }

        return 1 * currentMultiplier;
    }

    /// <summary>
    /// Expects a normalised range (between 0 and 1), which it uses to retrieve the appropriate correspoding score from calcualteScore().
    /// </summary>
    public void incrementScore(float increment)
    {
        float roundIncrement = Mathf.Round(increment * 10.0f) / 0.1f;
        newPoints = calcualteScore(roundIncrement);
        currentScore += newPoints;
        StartCoroutine(DisplayIncremenetCountdown("score", newPoints));
        //Debug.Log($"currentScore: {currentScore}");
        incrementMultiplier();
    }

    /// <summary>
    /// Modifies multiplier from incoming modificationValue, clamps to minMultiplier/maxMultiplier range, returns clamped value
    /// </summary>
    public float updateMultiplier(float modificationValue)
    {
        float updatedMultiplier = Mathf.Clamp(modificationValue, minMultiplier, maxMultiplier);
        //Debug.Log("updateMultiplier()");
        multiplierMod = currentMultiplier - updatedMultiplier;
        return updatedMultiplier;
    }

    /// <summary>
    /// Incremenets to the multiplier (ensuring clamped between min/max range)
    /// </summary>
    public void incrementMultiplier()
    {
        float increment = currentMultiplier + firewokMultiplier;
        currentMultiplier = updateMultiplier(increment);
        StartCoroutine(DisplayIncremenetCountdown("notScore", 0.0f));
    }

    /// <summary>
    /// Decrements to the multiplier (ensuring clamped between min/max range)
    /// </summary>
    public void decrementMultiplier()
    {
        float decrement = currentMultiplier - (missedMultiplierDecrement * firewokMultiplier);
        currentMultiplier = updateMultiplier(decrement);
        StartCoroutine(DisplayIncremenetCountdown("notScore", 0.0f));
    }
    
}
