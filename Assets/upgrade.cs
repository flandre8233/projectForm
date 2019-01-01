using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class upgrade : MonoBehaviour {
    enum levelInfo {
        SpeedUp,
        Missile,
        Double,
        Wave,
        Option,
        Shield
    }
    int cur_level;
	// Update is called once per frame
	void Update () {
        //測試用
        if (Input.GetKeyDown(KeyCode.Q)) {
            onGetLevelUpItem();
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            if (cur_level > 0) {
                onUpgrade();
            }
        }
    }

    //當得到升級道具時
    void onGetLevelUpItem() {
        //上溢會由speedup重來
        if (cur_level ++ >= 6) {
            cur_level = 1;
        }
        print("現在可以升級的道具為 " + curLevelToLevelInfo() );
    }

    //當確定升級時
    void onUpgrade() {
        upgradeSkill();
        //升級完就reset升級道具
        resetCurLevel();
    }

    //升級！
    void upgradeSkill() {
        switch (curLevelToLevelInfo()) {
            case levelInfo.SpeedUp:
                //speedUp level up script here
                break;
            case levelInfo.Missile:
                //Missile level up script here
                break;
            case levelInfo.Double:
                //Double level up script here
                break;
            case levelInfo.Wave:
                //Wave level up script here
                break;
            case levelInfo.Option:
                //Option level up script here
                break;
            case levelInfo.Shield:
                //Shield level up script here
                break;
            default:
                break;
        }
        print(curLevelToLevelInfo() + " Upgrade!");
    }

    //目前等級轉換器
    levelInfo curLevelToLevelInfo() {
        return (levelInfo)cur_level - 1;
    }

    //重置目前等級
    void resetCurLevel() {
        cur_level = 0;
    }
}
