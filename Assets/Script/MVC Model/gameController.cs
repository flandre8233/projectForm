using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : SingletonMonoBehavior<gameController> {

    [SerializeField]
    Camera camera;

    public List< GameObject > ant;

    [SerializeField]
     GameObject antPrefabs;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0)) {
            Vector3Int mousePos = gameModel.instance.worldToMapV3(camera.ScreenToWorldPoint(Input.mousePosition) );
            print(mousePos);

            Vector3 bakeCenter = ant[ 0 ].transform.position;
            Vector3 endPoint = mousePos;
            float angleDeg = (Mathf.Atan2(endPoint.y - bakeCenter.y, endPoint.x - bakeCenter.x) * 180 / Mathf.PI);

            
            if (angleDeg <= 45f && angleDeg > -45f ) {
                print("right");

            } else if (angleDeg <= -45f && angleDeg > -135f) {
                print("down");

            } else if (angleDeg <= -135f || angleDeg > 135f) {
                print("left");

            } else {
                print("up");
            }


            print(angleDeg);

            
            foreach (var item in ant) {
                item.GetComponent<Ant>().pathfindedList.Clear();
                item.GetComponent<Ant>().pathfindedListInt = pathfinding.StartBakeAllFloorToVector3Int(gameModel.instance.charWorldToMapV3(item.transform), mousePos);
                item.GetComponent<Ant>().startLerpToDestination();
            }
            

        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            ant.Add( Instantiate(antPrefabs, new Vector3(12, 0, 0), Quaternion.identity) );
            gameModel.instance.delayerValUpdate();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            for (int i = 0; i < 10; i++) {
                ant.Add(Instantiate(antPrefabs, new Vector3(12,0,0),Quaternion.identity ));
            }
            gameModel.instance.delayerValUpdate();

        }
        if (Input.GetKeyDown(KeyCode.C)) {
            for (int i = 0; i < 100; i++) {
                ant.Add(Instantiate(antPrefabs, new Vector3(12, 0, 0), Quaternion.identity));
            }
            gameModel.instance.delayerValUpdate();

        }
        if (Input.GetKeyDown(KeyCode.V)) {
            for (int i = 0; i < 1000; i++) {
                ant.Add(Instantiate(antPrefabs, new Vector3(12, 0, 0), Quaternion.identity));
            }
            gameModel.instance.delayerValUpdate();

        }
    }
}
