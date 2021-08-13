﻿// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CubicBezier
{
    public Vector2 p1 = new Vector2();
    public Vector2 p2 = new Vector2();
    public Vector2 p3 = new Vector2();
    public Vector2 p4 = new Vector2();
}

public class KanjiUtils
{

    // t must be a value from 0 - 1
    private static Vector2 GetPntOnCubicBezier(float t, CubicBezier cB)
    {
        var ti = 1 - t;
        Vector2 term1 = cB.p1 * (ti * ti * ti);
        Vector2 term2 = cB.p2 * (3 * ti * ti * t);
        Vector2 term3 = cB.p3 * (3 * ti * t * t);
        Vector2 term4 = cB.p4 * (t * t * t);
        Vector2 r = term1 + term2 + term3 + term4;
        return r;
    }

    private static List<Vector2> GetPntsOnCubicBezier(CubicBezier cB, int noOfPnts)
    {
        List<Vector2> pnts = new List<Vector2>();
        for (int i = 0; i < noOfPnts; i++)
        {
            pnts.Add(GetPntOnCubicBezier(i / (float)noOfPnts, cB));
        }
        return pnts;
    }

    private static float GetLengthOfCubicBezier(CubicBezier cB, float res = 0.1f)
    {
        float length = 0;
        Vector2 curPnt = cB.p1;
        for (float i = res; i <= 1; i += res)
        {
            Vector2 newPnt = GetPntOnCubicBezier(i, cB);
            length += (newPnt - curPnt).magnitude;
            curPnt = newPnt;
        }
        return length;
    }

    private struct PathInfo
    {
        public float[] lengths;
        public float[] offsets;
        public float totLength;
    };

    public static List<Vector2> GetPointsForVectorStroke(List<CubicBezier> vectorPaths, int pntsInStroke)
    {
        // get the bezier offsets

        PathInfo pthInfo = new PathInfo
        {
            lengths = new float[vectorPaths.Count],
            offsets = new float[vectorPaths.Count],
            totLength = 0f
        };

        int lstIdx = vectorPaths.Count - 1;
        for (int i = 0; i < vectorPaths.Count; i++)
        {
            pthInfo.lengths[i] = GetLengthOfCubicBezier(vectorPaths[i]);
            pthInfo.totLength += pthInfo.lengths[i];
            if (i > 0)
            {
                pthInfo.offsets[i] = pthInfo.lengths[i - 1] + pthInfo.offsets[i - 1];
            }
        }

        // get the points across the whole vector path
        List<Vector2> pnts = new List<Vector2>();
        int pthIdx = 0;
        for (int i = 0; i < pntsInStroke; i++)
        {
            float tPth = i / (float)pntsInStroke;
            float pPthScaled = tPth * pthInfo.totLength;
            int nxtPthIdx = pthIdx + 1;
            if (nxtPthIdx != vectorPaths.Count &&
               pPthScaled > pthInfo.offsets[nxtPthIdx])
            {
                pthIdx = nxtPthIdx;
            }
            float tVecPath = (pPthScaled - pthInfo.offsets[pthIdx]) / pthInfo.lengths[pthIdx];
            tVecPath = Mathf.Clamp(tVecPath, 0, 1);
            pnts.Add(GetPntOnCubicBezier(tVecPath, vectorPaths[pthIdx]));
        }
        return pnts;
    }

    public static float GetLengthForPnts(List<Vector2> points)
    {
        // get total length of line
        float totalDist = 0;
        for (int i = 1; i < points.Count; i++)
        {
            totalDist += (points[i] - points[i - 1]).magnitude;
        }
        return totalDist;
    }

    public static List<Vector2> GenRefPntsForPnts(List<Vector2> points, int noOfPoints = 5)
    {
        if (noOfPoints > points.Count || noOfPoints < 1) return new List<Vector2>();
        if (points.Count > 3)
        {
            List<Vector2> refPnts = new List<Vector2>();
            // get total length of line
            float totalDist = GetLengthForPnts(points);
            noOfPoints--; // add last point manually
            float increment = totalDist / noOfPoints;
            // add points
            for (int j = 0; j < noOfPoints; j++)
            {
                float currDist = 0;
                float targetDist = j * increment;
                for (int i = 1; i < points.Count; i++)
                {
                    currDist += (points[i] - points[i - 1]).magnitude;
                    if (currDist > targetDist)
                    {
                        refPnts.Add(points[i]);
                        break;
                    }
                }
            }
            refPnts.Add(points[points.Count - 1]);

            return refPnts;
        }
        return points;
    }

    public static List<Vector2> SVGToUnityCoords(List<Vector2> points)
    {
        return points.ConvertAll(p => new Vector2(p.x, -p.y));
    }

    public static List<Vector2> ScaleCoords(List<Vector2> points, float scale)
    {
        return points.ConvertAll(p => new Vector2(p.x * scale, p.y * scale));
    }


}