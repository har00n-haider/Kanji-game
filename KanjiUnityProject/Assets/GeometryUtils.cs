﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeometryUtils
{
    // Clamps a 1d length smoothly inside a 1d region
    public static float ClampLengthToRegion(float center, float length, float posEdge, float negEdge = 0)
    {
        bool inside = center >= negEdge && center <= posEdge;
        float halfLength = length / 2;
        if (inside)
        {
            // fully inside
            bool fullyinside = center + halfLength <= posEdge && center - halfLength >= negEdge;
            if (fullyinside) return center;
            // partially inside
            bool nearPosEdge = Mathf.Abs(center - posEdge) <= Mathf.Abs(center - negEdge);
            float insideAmount = nearPosEdge ? posEdge - center : center - negEdge;
            float remainingAmount = halfLength - insideAmount;
            return center + (nearPosEdge ? -remainingAmount : remainingAmount);
        }
        //fully outside
        else
        {
            return Mathf.Clamp(center, 0, posEdge) +
            (center >= posEdge ? -halfLength : center >= 0 ? 0 : halfLength);
        }
    }

    public static Vector3 NormalizePointToBoxPosOnly(Vector3 boxSize, Vector3 point) 
    {
        return new Vector3(
            Mathf.Clamp01(point.x / boxSize.x), 
            Mathf.Clamp01(point.y / boxSize.y), 
            Mathf.Clamp01(point.z / boxSize.z));
    }

    public static Vector2 NormalizePointToBoxPosOnly(Vector2 boxSize, Vector2 point)
    {
        return new Vector2(
            Mathf.Clamp01(point.x / boxSize.x),
            Mathf.Clamp01(point.y / boxSize.y));
    }

}

