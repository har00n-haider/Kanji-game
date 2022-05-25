using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;
using System.Linq;
using Manabu.Core;
using System;

/// <summary>
///  Contains all the information required to create and manage a group of 
///  stroke targets representing a character
/// </summary>
public class CharacterManager : MonoBehaviour
{
    // scale
    /// <summary>
    /// Width & height of the character in world units. Used to scale the 0 - 1 range of the Manabu character
    /// </summary>
    public Vector3 CharacterSize { get { return characterSize; } }
    [SerializeField]
    private Vector3 characterSize;
    public Vector3 CharacterCenter { get { return new Vector3(0.5f * characterSize.x, 0.5f * characterSize.y, 0.5f * characterSize.z); } }


    // state
    private List<CharacterStrokeTarget> strokes = new List<CharacterStrokeTarget>();
    private CharacterStrokeTarget curStroke { get { return curStrokeIdx < strokes.Count ? strokes[curStrokeIdx] : null; } }
    private int curStrokeIdx = 0;
    public bool completed { get; private set; } = false;

    // character data
    public Character Character { get; private set; } = null;
    [SerializeField]
    private char character;
    [SerializeField]
    private TextAsset databaseFile;

    // events
    public event Action OnCompleted;

    // refs

    public CharacterStrokeTarget strokeTargetPrefab;

    private void Start()
    {
    }

    private void Init(Character charData, List<Tuple<BeatManager.Beat, BeatManager.Beat>> beats)
    {
        this.Character = charData;

        Assert.IsFalse(beats.Count == charData.drawData.strokes.Count * 2);

        // generate the pairs of strokes that will take the input points, one stroke at a time
        for (int strokeId = 0; strokeId < charData.drawData.strokes.Count; strokeId++)
        {
            // assuming we get these in order
            CharacterStrokeTarget target = Instantiate(strokeTargetPrefab, transform);
            target.Init(beats[strokeId].Item1, beats[strokeId].Item2, CharacterSize, charData.drawData.strokes[strokeId].points);
            strokes.Add(target);
        };

        // start the looking for the first stroke
        curStrokeIdx = 0;
        strokes[0].inpStroke.active = true;
    }

    private void Update()
    {
        if (completed || strokes.Count == 0) return;
        UpdateInput();
        // process current stroke, move to the next if done
        if (curStroke.completed)
        {
            curStroke.EvaluateStroke();
            curStroke.inpStroke.active = false;
            if (curStroke == strokes.Last())
            {
                completed = true;
                return;
            }
            else
            {
                curStrokeIdx++;
                curStroke.inpStroke.active = true;
            }
        };
        // deal with a completed character
        if (completed) OnCompleted?.Invoke();
    }

    private void UpdateInput()
    {
        // populate line
        bool buttonPressed = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
        if (buttonPressed)
        {
            // convert mouse position to a point on the kanji plane 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hit = GetPlane().Raycast(ray, out float enter);
            if (hit)
            {
                // normalize the input points for correct comparison with ref stroke
                Vector3 worldPoint = ray.direction * enter + ray.origin;
                Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                curStroke.inpStroke.AddPoint(localPoint);
            }

        }
        // clear line
        bool buttonReleased = Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);
        if (buttonReleased)
        {
            curStroke.inpStroke.Complete();
        }
    }

    // Get the plane on which the the character lies
    private Plane GetPlane()
    {
        // create the plane on which the character will be drawn
        Vector3 planePoint = transform.TransformPoint(CharacterCenter);
        Vector3 planeDir = -gameObject.transform.forward;
        return new Plane(planeDir.normalized, planePoint);
    }

}
