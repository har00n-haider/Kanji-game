using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a single location where app-global events are defined.
/// The intention is to centralise all of the global events which can be raised (sent) and
/// subscribed-to (received), since events are generic things which happen during the life-cycle of the app.
///
/// <example>
/// This example shows how to SUBSCRIBE to (receive) an event.
/// When OnPlayerConnectedClient is raised (per the next example), OnPlayerConnectedClientHandler() will be invoked.
/// <code>
/// AppEvents.OnPlayerConnectedClient += OnPlayerConnectedClientHandler();
/// </code>
/// </example>
///
/// <example>
/// This example shows how to RAISE (send) the 'OnPlayerConnectedClient' event.
/// Note the use of the ?. idiom, which avoids errors if AppEvents.OnPlayerConnectedClient is null.
/// <code>
/// AppEvents.OnPlayerConnectedClient?.Invoke();
/// </code>
/// </example>
/// </summary>
public class AppEvents
{
    /// <summary>
    /// Event for when a level starts.
    /// </summary>
    public static Action OnStartLevel;

    /// <summary>
    /// Event for when a level completes.
    /// </summary>
    public static Action OnEndLevel;

    /// <summary>
    /// Event for when a firework is spawned.
    /// Arg1 = GameObject = GameObject being spawned.
    /// Arg2 = Vector3 = Force of spawning.
    /// </summary>
    public static Action<GameObject, Vector3> OnSpawnFirework;

    /// <summary>
    /// Event for when an Explosion is spawned.
    /// Arg1 = GameObject = GameObject being spawned.
    /// </summary>
    public static Action<GameObject> OnBeatClick;

    /// <summary>
    /// Event for when a firework is clicked on by the player.
    /// Arg1 = GameObject = Firework which was clicked on & hit detected.
    /// </summary>
    public static Action<GameObject> OnFireworkMissed;
    public static Action<GameObject> OnSpawnExplosion;

}
