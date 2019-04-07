using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public  class globalUpdateManager : SingletonMonoBehavior<globalUpdateManager> {

    [Range(0, 15)]
    [SerializeField]
     float timeSpeed = 1;

    public delegate void updateDelegate();
    public updateDelegate globalUpdateDg;



    // Update is called once per frame
    void Update() {
        updateDeltaTimeData();
        if (globalUpdateDg != null) {
            globalUpdateDg.Invoke();
        }
    }

    public void setTimeSpeed()
    {
        Time.timeScale = timeSpeed;
    }

    void updateDeltaTimeData() {
        globalVarManager.deltaTime = Time.deltaTime;
    }

    /// <summary>
    /// 把function註冊在每幀執行的delegate裡
    /// </summary>
    public void registerUpdateDg(updateDelegate obj) {
        globalUpdateDg += obj;
    }
    /// <summary>
    /// 取消function在每幀執行的delegate的註冊
    /// </summary>
    public void UnregisterUpdateDg(updateDelegate obj) {
        globalUpdateDg -= obj;
    }
    public static void ClearDelegate(ref updateDelegate delg) {
        System.Delegate[] delegates = delg.GetInvocationList();
        foreach (System.Delegate d in delegates) {
            delg -= (updateDelegate)d;
        }
    }

    public void startGlobalTimer(float time, updateDelegate onEndFunction)
    {
        StartCoroutine(globalTimer(time,onEndFunction));
    }

    IEnumerator globalTimer(float time,updateDelegate onEndFunction)
    {
        yield return new WaitForSeconds(time);
        onEndFunction();
    }

}
