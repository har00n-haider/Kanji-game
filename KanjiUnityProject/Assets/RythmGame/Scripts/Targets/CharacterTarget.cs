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
    // character pos/scale
    /// <summary>
    /// Width & height of the character in world units. Used to scale the 0 - 1 range of the Manabu character
    /// </summary>
    private Vector3 CharacterSize { get; set; }
    /// <summary>
    /// Center of che chan
    /// </summary>
    public Vector3 CharacterCenter { get { return new Vector3(0.5f * CharacterSize.x, 0.5f * CharacterSize.y, 0.5f * CharacterSize.z); } }

    // state
    public List<CharacterStrokeTarget> Strokes = new List<CharacterStrokeTarget>();
    public CharacterStrokeTarget CurrentStroke { get { return CurrentStrokeIdx < Strokes.Count ? Strokes[CurrentStrokeIdx] : null; } }
    public int CurrentStrokeIdx { get; private set; } = 0;
    public bool Completed { get { return Strokes.TrueForAll(s => s.Completed); } } 
    public bool Pass { get { return Strokes.TrueForAll(s => s.Pass); } } 


    // character data
    public Character Character { get; private set; } = null;

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
    }

    private void Update()
    {

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
            strokeTarget.OnStrokeCompleted += UpdateStrokes;
            CurrentStrokeIdx++;
        }
    }

    public void UpdateStrokes(CharacterStrokeTarget strokeTarget)
    {
        if (Completed)
        {
            AppEvents.OnCharacterCompleted?.Invoke(this);
            gameObject.SetActive(false);
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

        private void OnDrawGizmos()
        {
            // Box enclosing the character
            DrawBox(transform.TransformPoint(CharacterCenter), transform.rotation, CharacterSize, new Color(0, 1 ,0 , 0.5f));

            // draw debug strokes
            DrawStrokePair();

            if (Strokes.Count > 0)
            {
                // HACK: crashes when <= is used
                for (int i = 0; i < CurrentStrokeIdx; i++)
                {
                    DrawStrokeEvaluation(Strokes[i]);
                }
            }
        }

        private void DrawStrokeEvaluation(CharacterStrokeTarget sp)
        {
            var config = GameManager.Instance.TargetSpawner.WritingConfig;

            if (sp.Completed)
            {
                for (int i = 0; i < config.noRefPointsInStroke; i++)
                {
                    float radius = CharacterSize.magnitude / 110.0f;
                    Gizmos.color = Color.gray;
                    var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.keyPoints[i].x, sp.refStroke.keyPoints[i].y, CharacterCenter.z));
                    Gizmos.DrawSphere(refPnt, radius);
                    Gizmos.color = new Color(0, 0, 0, 0.1f);
                    Gizmos.DrawSphere(refPnt, config.compThreshLoose);
                    Gizmos.DrawSphere(refPnt, config.compThreshTight);
                    Gizmos.color = sp.Pass ? Color.green : Color.red;
                    // tight dist color
                    //Gizmos.color = sp.tightPointIdx == i ? new Color(1, 0, 1) : Gizmos.color; // purple
                    Vector3 inpPnt = Vector3.zero;
                    if(sp.inpStroke.keyPoints.Count > 0)
                    {
                        inpPnt = transform.TransformPoint(new Vector3(sp.inpStroke.keyPoints[i].x, sp.inpStroke.keyPoints[i].y, CharacterCenter.z));
                    }
                    Gizmos.DrawSphere(inpPnt, radius);
                    // connect the two
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(refPnt, inpPnt);
                }
            }
        }

        private void DrawStrokePair()
        {
            Action<List<Vector2>, Color> drawStroke = (l, c) =>
            {
                for (int i = 1; i < l.Count; i++)
                {
                    Vector3 start = transform.TransformPoint(new Vector3(l[i - 1].x, l[i - 1].y, CharacterCenter.z));
                    Vector3 end = transform.TransformPoint(new Vector3(l[i].x, l[i].y, CharacterCenter.z));
                    Debug.DrawLine(start, end, c);
                }
            };

            foreach (var s in Strokes)
            {
                drawStroke(s.refStroke.points, Color.green);
            }

            if (CurrentStroke != null) drawStroke(CurrentStroke.inpStroke.points, Color.blue);
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
 
