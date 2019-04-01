using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mine : building {
    bool abortion = false;

    [ SerializeField]
    int resource ;

    public int checkMineResource() {
        return resource;
    }


    // Use this for initialization
    public override void init (Vector2Int initObjectMapV2) {
        base.init(initObjectMapV2);
        mine otherMineAlreadyHave = gameModel.instance.getFloorDatas(InMapV3Pos).mine;
        if (otherMineAlreadyHave) {
            otherMineAlreadyHave.OnCombineOtherMineResource(resource);
            abortion = true;
            Destroy(gameObject);
            return;
        }
        gameModel.instance.getFloorDatas(InMapV3Pos).RegisterMineData(this);
        gameModel.instance.mineList.Add(this);
    }
    public void init(Vector2Int initObjectMapV2, int startResource) {
        resource = startResource;
        init(initObjectMapV2);
    }

    void OnDestroy() {
        if (abortion) {
            return;
        }
        gameModel.instance.mineList.Remove(this);
        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterMineData();
        print("Remove");

    }

    void OnDead() {
        Destroy(gameObject);

    }

    public void OnCombineOtherMineResource(int otherResource) {
        resource += otherResource;
    }

    public int OnBeMining(int take) {
        resource -= take;
        //check resource
        if (resource <= 0) {
            OnDead();
            return resource + take;
        }
        return take;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
