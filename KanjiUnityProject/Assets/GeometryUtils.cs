using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Random = UnityEngine.Random;

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

    /// <summary>
    /// Random position in axis aligned box
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPositionInBounds(Bounds bounds) 
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public static void PopulateCirclePoints3DXY(ref Vector3[] points, float radius, Vector3 center) 
    {
        int lastPntIdx = points.Length - 1;
        for (int i = 0; i <= lastPntIdx; i++)
        {
            float angle = ((float)i/lastPntIdx) * MathF.PI * 2;
            points[i].x = radius * Mathf.Cos(angle) + center.x;
            points[i].y = radius * Mathf.Sin(angle) + center.y;
            points[i].z = center.z;
        }
    }

    public static void PopulateLinePoints(ref Vector3[] points, Vector3 start, Vector3 end) 
    {
        int lastPntIdx = points.Length - 1;
        Vector3 segmentVector = (end - start) / lastPntIdx;
        for (int i = 0; i <= lastPntIdx; i++)
        {
            points[i] = start + segmentVector * i;
        }
    }


}

