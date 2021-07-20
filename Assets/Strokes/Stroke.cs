using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Will take the input and draw stroke and generate key points
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Stroke : MonoBehaviour
{
    public LineRenderer line;
    public List<Vector2> refPoints;
    public float width = 0.1f;

    protected virtual void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public virtual void Init() 
    {
    }

}
