using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class building : MonoBehaviour {

    public Vector2Int InMapV3Pos;
    public int UID;

    // Use this for initialization
    public virtual void init(Vector2Int initObjectMapV2) {
        InMapV3Pos = initObjectMapV2;
        alignmentToMap();
        UID = gameModel.instance.UIDRequest();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void alignmentToMap() {
        transform.position = gameModel.instance.mapV3ToWorldPos(InMapV3Pos);
    }

    private void Start() {
        //init();
    }

}
