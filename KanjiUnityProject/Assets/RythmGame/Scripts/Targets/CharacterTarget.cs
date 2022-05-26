using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Manabu.Core;
using System;


/// <summary>
///  Contains all the information required to create and manage a group of 
///  stroke targets representing a character. 
/// </summary>
public class CharacterTarget : MonoBehaviour
{
    // scale
    /// <summary>
    /// Width & height of the character in world units. Used to scale the 0 - 1 range of the Manabu character
    /// </summary>
    private Vector3 CharacterSize { get; set; }
    public Vector3 CharacterCenter { get { return new Vector3(0.5f * CharacterSize.x, 0.5f * CharacterSize.y, 0.5f * CharacterSize.z); } }

    // state
    public List<CharacterStrokeTarget> Strokes = new List<CharacterStrokeTarget>();
    public CharacterStrokeTarget CurrentStroke { get { return CurrentStrokeIdx < Strokes.Count ? Strokes[CurrentStrokeIdx] : null; } }
    public int CurrentStrokeIdx { get; private set; } = 0;
    public bool Completed { get; private set; } = false;

    // character data
    public Character Character { get; private set; } = null;

    // events
    public event Action OnCompleted;

    // refs
    public CharacterStrokeTarget strokeTargetPrefab;
    public List<Tuple<BeatManager.Beat, BeatManager.Beat>> Beats { get; private set; } = null;

    public void Init(Character character, Vector3 CharacterSize, List<Tuple<BeatManager.Beat, BeatManager.Beat>> beats)
    {
        Assert.IsFalse(beats.Count == character.drawData.strokes.Count * 2);
        Beats = beats;
        Character = character;
        this.CharacterSize = CharacterSize;
        CurrentStrokeIdx = 0;
        //Strokes[0].inpStroke.active = true;
    }

    public void CreateNextStroke()
    {
        if(CurrentStrokeIdx < Beats.Count){
            CharacterStrokeTarget strokeTarget = Instantiate(
                strokeTargetPrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<CharacterStrokeTarget>();
            strokeTarget.Init(
                Beats[CurrentStrokeIdx].Item1, 
                Beats[CurrentStrokeIdx].Item2, 
                CharacterSize, 
                Character.drawData.strokes[CurrentStrokeIdx].points,
                this);
            Strokes.Add(strokeTarget);
            CurrentStrokeIdx++;
        }
    }

    // Get the plane on which the the character lies
    public Plane GetCharacterPlane()
    {
        // create the plane on which the character will be drawn
        Vector3 planePoint = transform.TransformPoint(CharacterCenter);
        Vector3 planeDir = -transform.forward;
        return new Plane(planeDir.normalized, planePoint);
    }

}
 
