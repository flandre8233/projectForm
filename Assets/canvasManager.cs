using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class canvasManager : SingletonMonoBehavior<canvasManager> {
    [SerializeField]
    Text antNumberText;
    [SerializeField]
    Text enemyAntNumberText;
    public void init() {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
    }
    // Update is called once per frame
    void ToUpdate () {
        updateNumberText();
    }

    void updateNumberText() {
        antNumberText.text = gameModel.instance.antList.Count.ToString();
        enemyAntNumberText.text = gameModel.instance.ant_enemyList.Count.ToString();
    }
    private void OnDestroy() {
        
    }
}
