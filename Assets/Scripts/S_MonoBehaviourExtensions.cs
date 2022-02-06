using System.Collections;
using UnityEngine;
using System;

public static class S_MonoBehaviourExtensions
{
    public static void CallWithDelay(this MonoBehaviour mono, Action method, float delay)
    {
        mono.StartCoroutine(CallWithDelayRoutine(method, delay));
    }

    static IEnumerator CallWithDelayRoutine(Action method, float delay)
    {
        yield return new WaitForSeconds(delay);
        method();
    }
}
