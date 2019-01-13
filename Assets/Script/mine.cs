using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mine : building {

    public int resource = 1000;

    // Use this for initialization
    public override void init () {
        base.init();
        gameModel.instance.getFloorDatas(InMapV3Pos).RegisterMineData(this);

    }
	


	// Update is called once per frame
	void Update () {
		
	}
}
