using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct stringBool
{
    public int index;
    public string name;
    public bool value;
}

public class AutomaticComfySceneScript : MonoBehaviour
{
    public List<stringBool> bools;

    public UnityEvent eventer;

    public void DoFunction(Action callback, bool curBool)
    {
        if (curBool) return;
        callback.Invoke();
    }
}
