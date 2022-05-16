using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Utils
{
    // cloner for simple objects
    public static T Clone<T>(this T source)
    {
        var serialized = JsonUtility.ToJson(source);
        return JsonUtility.FromJson<T>(serialized);
    }

    public static T PickRandom<T>(this List<T> source)
    {
        int idx = UnityEngine.Random.Range(0, source.Count() - 1);
        return source[idx];
    }
}