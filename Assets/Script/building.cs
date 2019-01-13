using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class building : MonoBehaviour {

    public Vector2Int InMapV3Pos;
    public int UID;

    // Use this for initialization
    public virtual void init() {
        alignmentToMap();
        UID = gameModel.instance.UIDRequest();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void alignmentToMap() {
        print("do");
        transform.position = gameModel.instance.mapV3ToWorldPos(InMapV3Pos);
    }

    private void Start() {
        init();
    }

}
