using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//這是為unity的lerp系統加以開發的系統，用者要先命名一個class承繼lerpMoveSystem，再把d oPart(ref Vector3 startVar, ref Vector3 endVar, ref float curvedValue) override掉如下例子
//然後把update放入每幀都會更新的程式碼 例：void Update()就可

public class testVector3Lerp : vector3Lerp {
    public override void startExtraCode() {
    }
    public override void endExtraCode() {
        //TESTLERP.instance.counter++;
    }
}

public class vector3Lerp : lerpMoveSystem<Vector3> {
    public Vector3 var;
    public override Vector3 doPart(ref Vector3 startVar, ref Vector3 endVar, ref float curvedValue) {
        var = Vector3.Lerp(startVar, endVar, curvedValue);
        return var;
    }
    public override void startExtraCode() {
    }
    public override void endExtraCode() {
    }
}

public class QuaternionLerp : lerpMoveSystem<Quaternion> {
    public Quaternion var;
    public override Quaternion doPart(ref Quaternion startVar, ref Quaternion endVar, ref float curvedValue) {
        var = Quaternion.Slerp(startVar, endVar, curvedValue);
        return var;
    }
    public override void startExtraCode() {
    }
    public override void endExtraCode() {
    }
}

public class FloatLerp : lerpMoveSystem<float> {
    public float var;
    public override float doPart(ref float startVar, ref float endVar, ref float curvedValue) {
        var = Mathf.Lerp(startVar, endVar, curvedValue);
        return var;
    }
    public override void startExtraCode() {
    }
    public override void endExtraCode() {
    }
}


public abstract class lerpMoveSystem<T> {
    bool _isLerping = false;
    float moveNeedTime = 0f;
    float lerpTime;
    float _curvedValue;
    public delegate void lerpMoveDelegate();
    lerpMoveDelegate onStartDg;
    lerpMoveDelegate onEndDg;
    AnimationCurve aniCurve;

    public T startVar;
    public T endVar;

    public bool isLerping {
        get {
            return _isLerping;
        }
    }


    public void startLerp(T start, T end, AnimationCurve curve, float moveTime, lerpMoveDelegate onStartFunction, lerpMoveDelegate onEndFunction) {
        if (_isLerping) {
            return;
        }
        onStartDg = null;
        onEndDg = null;
        _isLerping = true;
        startVar = start;
        endVar = end;
        aniCurve = curve;
        moveNeedTime = moveTime;
        lerpTime = 0;
        onStartDg += onStartFunction;
        onEndDg += onEndFunction;
        if (onStartDg != null) {
            onStartDg.Invoke();
        }
        startExtraCode();
    }

    public void startLerp(T start, T end, AnimationCurve curve, float moveTime) {
        if (_isLerping) {
            return;
        }
        _isLerping = true;
        startVar = start;
        endVar = end;
        aniCurve = curve;
        moveNeedTime = moveTime;
        lerpTime = 0;
        startExtraCode();
    }

    public void startLerp(T start, T end, float moveTime, lerpMoveDelegate onStartFunction, lerpMoveDelegate onEndFunction) {
        if (_isLerping) {
            return;
        }
        onStartDg = null;
        onEndDg = null;
        _isLerping = true;
        startVar = start;
        endVar = end;
        moveNeedTime = moveTime;
        lerpTime = 0;
        onStartDg += onStartFunction;
        onEndDg += onEndFunction;
        if (onStartDg != null) {
            onStartDg.Invoke();
        }
        startExtraCode();
    }

    public void startLerp(T start, T end, float moveTime) {
        if (_isLerping) {
            return;
        }
        _isLerping = true;
        startVar = start;
        endVar = end;
        moveNeedTime = moveTime;
        lerpTime = 0;
        startExtraCode();
    }

    void endLerp() {
        _isLerping = false;
     
        endExtraCode();

        if (onEndDg != null) {
            onEndDg();
        }
    }

    public abstract void startExtraCode();
    public abstract void endExtraCode();

    public abstract T doPart(ref T startVar, ref T endVar, ref float curvedValue);

    public T update() {
        if (!_isLerping) {
            return default(T);
        }
        T outPutData;

            lerpTime += globalVarManager.deltaTime / moveNeedTime;
        _curvedValue = lerpTime;
        if (aniCurve != null) {
            _curvedValue = aniCurve.Evaluate(lerpTime);
        }

        //do part
        outPutData = doPart(ref startVar, ref endVar, ref _curvedValue);

        if (lerpTime >= 1) {
            endLerp();
        }
        return outPutData;
    }
}