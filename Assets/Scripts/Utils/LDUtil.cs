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
    /// Wait time seconds before calling the given method as an action.
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

    //overload that allows 
    public static IEnumerator Wait<T1>(Action<T1> action, T1 parameter, float time)
    {

        //wait for 0.5 seconds
        yield return new WaitForSeconds(time);
        //call the given action.
        action(parameter);
    }
}
