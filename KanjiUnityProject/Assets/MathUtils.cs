using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class MathUtils
{
    // Clamps value between 0 and 1 and returns value
    public static double Clamp01(double value)
    {
        if (value < 0F)
            return 0F;
        else if (value > 1F)
            return 1F;
        else
            return value;
    }

    // Interpolates between /a/ and /b/ by /t/. /t/ is clamped between 0 and 1.
    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * Clamp01(t);
    }

    // Interpolates between /a/ and /b/ by /t/ without clamping the interpolant.
    public static double LerpUnclamped(double a, double b, double t)
    {
        return a + (b - a) * t;
    }

    // Calculates the ::ref::Lerp parameter between of two values.
    public static double InverseLerp(double a, double b, double value)
    {
        if (a != b)
            return Clamp01((value - a) / (b - a));
        else
            return 0.0f;
    }
}