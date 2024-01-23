using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains various methods 
/// that can be helpful during game design.
/// </summary>
public class LDUtil : MonoBehaviour
{
    /// <summary>
    /// Wait <paramref name="time"> seconds before calling <paramref name="action">.
    /// </summary>
    /// <param name="action">the action to call</param>
    /// <param name="time">time until we call the action</param>
    /// <returns></returns>
    public static IEnumerator Wait(Action action, float time)
    {

        //wait for 0.5 seconds
        yield return new WaitForSeconds(time);
        //call the given action.
        action();
    }

    //overload that allows a parameter to be passed with the wait method.
    public static IEnumerator Wait<T1>(Action<T1> action, T1 parameter, float time)
    {

        //wait for 0.5 seconds
        yield return new WaitForSeconds(time);
        //call the given action.
        action(parameter);
    }

    /// <summary>
    /// Waits <paramref name="frames"/> frames until it calls the provided <paramref name="action"/>
    /// </summary>
    /// <param name="action"></param>
    /// <param name="frames"></param>
    /// <returns></returns>
    public static IEnumerator WaitFrames(Action action, int frames)
    {
        while (frames > 0)
        {
            frames--;
            yield return null;
        }
        action();
    }
}
