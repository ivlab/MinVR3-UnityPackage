using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class FSMCallback
{
    public UnityEvent callback;

    public void Invoke()
    {
        callback.Invoke();
    }

    public override string ToString()
    {
        string s = "";
        if (callback.GetPersistentEventCount() == 0) {
            s = "(null)";
        } else {
            for (int i = 0; i < callback.GetPersistentEventCount(); i++) {
                s += callback.GetPersistentMethodName(i) + "(); ";

                //s += callback.GetPersistentTarget(i).name + "." + callback.GetPersistentMethodName(i) + "(); ";
            }
        }
        return s;
    }
}
