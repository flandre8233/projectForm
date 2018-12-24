using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameView : SingletonMonoBehavior<gameView> {

	public void init() {
        canvasManager.instance.init();
    }
}
