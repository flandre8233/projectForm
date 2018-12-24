using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : SingletonMonoBehavior<gameController> {

    [SerializeField]
    Camera camera;

    [SerializeField]
    GameObject antPrefabs;
    [SerializeField]
    GameObject ant_enemyPrefabs;
    [SerializeField]
    GameObject ant_enemyPrefabs_big;

    float timer1;
    float timer2;

    // Use this for initialization
    void Start() {
        gameModel.instance.init();
        gameView.instance.init();

        GameObject[] antGameObject = GameObject.FindGameObjectsWithTag("Ant");
        foreach (var item in antGameObject) {
            Ant itemAnt = item.GetComponent<Ant>();
            if (itemAnt.isFriendly) {
                gameModel.instance.antList.Add(itemAnt);
            } else {
                gameModel.instance.ant_enemyList.Add(itemAnt);
            }
        }

        for (int i = 0; i < 0; i++) {
            gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(18, 0, 0), Quaternion.identity).GetComponent<Ant>());
        }
        for (int i = 0; i < 0; i++) {
            gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(3, 0, 0), Quaternion.identity).GetComponent<Ant>());
        }
        for (int i = 0; i < 0; i++) {
            gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs_big, new Vector3(18, 0, 0), Quaternion.identity).GetComponent<Ant>());
        }
        gameModel.instance.delayerValUpdate();

        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
    }

    void ToUpdate() {
        autoSpawn();
        if (Input.GetMouseButtonDown(0)) {
            Vector2Int mousePos = gameModel.instance.worldToMapV3(camera.ScreenToWorldPoint(Input.mousePosition));
            print(mousePos);

            Vector3 bakeCenter = gameModel.instance.antList[ 0 ].transform.position;
            Vector3 endPoint = new Vector3(mousePos.x, mousePos.y,0);
            float angleDeg = (Mathf.Atan2(endPoint.y - bakeCenter.y, endPoint.x - bakeCenter.x) * 180 / Mathf.PI);


            if (angleDeg <= 45f && angleDeg > -45f) {
                print("right");

            } else if (angleDeg <= -45f && angleDeg > -135f) {
                print("down");

            } else if (angleDeg <= -135f || angleDeg > 135f) {
                print("left");

            } else {
                print("up");
            }


            print(angleDeg);


            foreach (var item in gameModel.instance.antList) {
                item.cutOffCurMovement();
                item.Destination = mousePos;
                item.startLerpToDestination();
            }


        }

        if (Input.GetKeyDown(KeyCode.G)) {
            for (int i = 0; i < gameModel.instance.ant_enemyList.Count; i++) {
                Destroy(gameModel.instance.ant_enemyList[i].gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<Ant>());
            gameModel.instance.delayerValUpdate();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            for (int i = 0; i < 10; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            for (int i = 0; i < 100; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();

        }
        if (Input.GetKeyDown(KeyCode.V)) {
            for (int i = 0; i < 1000; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(3, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }


        if (Input.GetKeyDown(KeyCode.A)) {
            gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<Ant>());
            gameModel.instance.delayerValUpdate();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            for (int i = 0; i < 10; i++) {
                gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            for (int i = 0; i < 100; i++) {
                gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();

        }
        if (Input.GetKeyDown(KeyCode.F)) {
            for (int i = 0; i < 1000; i++) {
                gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(18, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
    }

    int spawnNumber = 15;
    void autoSpawn() {
        timer1 += globalVarManager.deltaTime;
        timer2 += globalVarManager.deltaTime;
        if (timer1 - 0.5f > 0) {
            timer1 = 0;
            for (int i = 0; i < spawnNumber; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(3, 0, 0), Quaternion.identity).GetComponent<Ant>());
                gameModel.instance.delayerValUpdate();
            }
         
        }
        if (timer2 - 0.5f > 0) {
            timer2 = 0;
            for (int i = 0; i < spawnNumber; i++) {
                gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(18, 0, 0), Quaternion.identity).GetComponent<Ant>());
                gameModel.instance.delayerValUpdate();
            }
   
        }

    }

}
