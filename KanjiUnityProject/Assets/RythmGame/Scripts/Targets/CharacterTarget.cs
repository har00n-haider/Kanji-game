using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Manabu.Core;
using System;
using System.Linq;
using TMPro;

/// <summary>
///  Contains all the information required to create and manage a group of 
///  stroke targets representing a character. 
///  
///  All positions and transforms under this character should be local 
///  to this game objects coordinate system. This includes:
///  
///  - line points for all the strokes
///  - keyPoints for all the strokes
///  - follow target position
///  - 
/// 
/// </summary>
public class CharacterTarget : MonoBehaviour
{
    // character pos/scale
    /// <summary>
    /// Width & height of the character in world units. Used to scale the 0 - 1 range of the Manabu character
    /// </summary>
    public Vector3 CharacterSize { get; set; }
    /// <summary>
    /// Center of the character, as opposed to the position of this game object
    /// </summary>
    public Vector3 CharacterCenter { get { return new Vector3(0.5f * CharacterSize.x, 0.5f * CharacterSize.y, 0.5f * CharacterSize.z); } }

    // state
    public List<CharacterStroke> Strokes = new List<CharacterStroke>();
    public bool Completed { get { return Strokes.TrueForAll(s => s.Completed); } }
    public bool Pass { get { return Strokes.TrueForAll(s => s.Pass); } }
    private int strokeCounter = 0;
    private bool playedPassEffect = false;
    private bool timedDeactivate = false;
    private float timedDeactivateTimer = 0.0f;
    public bool StrokesVisible { get; private set; } = false;

    // character data
    public Character Character { get; private set; } = null;

    // refs
    public CharacterStroke strokeTargetPrefab;
    public List<Tuple<Beat, Beat>> Beats { get; private set; } = null;
    private CharacterConfig config;
    public Beat StartBeat { get { return Beats.First().Item1; } }
    public Beat EndBeat { get { return Beats.Last().Item2; } }
    [SerializeField]
    private TextMeshPro backgroundText;

    // effects
    [SerializeField]
    private Effect characterPassEffect;

    public void Init(Character character, List<Tuple<Beat, Beat>> beats, CharacterConfig config)
    {
        Assert.IsFalse(beats.Count == character.drawData.strokes.Count * 2);
        Beats = beats;
        Character = character;
        this.CharacterSize = config.CharacterSize;
        this.config = config;
        name = "CharacterTarget - " + character.literal;
        backgroundText.text = character.romaji.ToUpper();
        backgroundText.gameObject.SetActive(false);
    }

    private void Update()
    {
        float yOffset = 0.3f * CharacterSize.y;
        backgroundText.transform.localPosition = new Vector3(CharacterCenter.x, CharacterCenter.y + yOffset, CharacterSize.z);
        if (timedDeactivate) timedDeactivateTimer += Time.deltaTime;
        if (timedDeactivateTimer > config.hangaboutTimeCharacter) gameObject.SetActive(false);
    }

    public void CreateNextStroke()
    {
        if (!StrokesVisible)
        {
            StrokesVisible = true;
            backgroundText.gameObject.SetActive(true);
        }

        if (strokeCounter < Beats.Count)
        {
            CharacterStroke characterStroke = Instantiate(
                strokeTargetPrefab,
                transform.position,
                Quaternion.identity,
                transform).GetComponent<CharacterStroke>();
            characterStroke.Init(
                Beats[strokeCounter].Item1,
                Beats[strokeCounter].Item2,
                CharacterSize,
                strokeCounter,
                this,
                config);
            Strokes.Add(characterStroke);
            characterStroke.OnStrokeCompleted += UpdateStrokes;
            strokeCounter++;
        }
    }

    public void UpdateStrokes(CharacterStroke strokeTarget)
    {
        if (Completed)
        {
            if (Pass && !playedPassEffect)
            {
                var c = Instantiate(characterPassEffect, transform.position, Quaternion.identity).GetComponent<CharacterPassedEffect>();
                c.Init(CharacterSize, this, config, Color.green);
                playedPassEffect = true;
            }

            AppEvents.OnCharacterCompleted?.Invoke(this);
            timedDeactivate = true;
            Destroy(gameObject, 3);
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



#if UNITY_EDITOR

    public bool drawDebug = false;

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;
        // Box enclosing the character
        DrawBox(transform.TransformPoint(CharacterCenter), transform.rotation, CharacterSize, new Color(0, 1, 0, 0.5f));
        // draw debug strokes
        DrawStroke();
    }

    private void DrawStroke()
    {
        foreach (var s in Strokes)
        {
            var l = s.refStroke.points;
            var c = Color.green;
            for (int i = 1; i < l.Count; i++)
            {
                Vector3 start = transform.TransformPoint(new Vector3(l[i - 1].x, l[i - 1].y, CharacterCenter.z));
                Vector3 end = transform.TransformPoint(new Vector3(l[i].x, l[i].y, CharacterCenter.z));
                Debug.DrawLine(start, end, c);
            }
        }
    }

    public void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
    {
        // create matrix
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS(pos, rot, scale);

        var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
        var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
        var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
        var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
        var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
        var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
        var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

        Debug.DrawLine(point1, point2, c);
        Debug.DrawLine(point2, point3, c);
        Debug.DrawLine(point3, point4, c);
        Debug.DrawLine(point4, point1, c);

        Debug.DrawLine(point5, point6, c);
        Debug.DrawLine(point6, point7, c);
        Debug.DrawLine(point7, point8, c);
        Debug.DrawLine(point8, point5, c);

        Debug.DrawLine(point1, point5, c);
        Debug.DrawLine(point2, point6, c);
        Debug.DrawLine(point3, point7, c);
        Debug.DrawLine(point4, point8, c);
    }

#endif

}

