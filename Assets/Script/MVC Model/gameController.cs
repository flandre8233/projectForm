using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : SingletonMonoBehavior<gameController> {

    [SerializeField]
    Camera camera;

    [SerializeField]
    GameObject[] ant;

    // Use this for initialization
    void Start () {
        ant = GameObject.FindGameObjectsWithTag("Ant");

    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0)) {
            Vector3Int mousePos = gameModel.instance.worldToMapV3(camera.ScreenToWorldPoint(Input.mousePosition) );
            print(mousePos);

            foreach (var item in ant) {
                item.GetComponent<Ant>().pathfindedList.Clear();
                item.GetComponent<Ant>().pathfindedList = pathfinding.StartBakeAllFloorToVector3Int(gameModel.instance.charWorldToMapV3(item.transform), mousePos);
                item.GetComponent<Ant>().startLerpToDestination();
            }

      
        }

	}
}
