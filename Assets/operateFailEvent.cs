using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class operateFailEvent : SingletonMonoBehavior<operateFailEvent> {
    public UnityEvent OnNoResourceAble;
    public UnityEvent OnLimitFull;

    public void tryDoInvoke(UnityEvent unityEvent) {
        if (unityEvent != null) {
            unityEvent.Invoke();
        }
    }

    public void logText(string log) {
        print("Log : " + log);
    }
}
