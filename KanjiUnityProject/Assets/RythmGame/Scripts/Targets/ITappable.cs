using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;

/// <summary>
/// Inteface for dealing with game objects that can be tapped on a beat
/// </summary>
public interface ITappable 
{
    public abstract void HandleBeatResult(Result result);
    public double BeatTimeStamp { get; }
}
