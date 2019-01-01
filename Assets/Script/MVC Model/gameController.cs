using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : SingletonMonoBehavior<gameController> {

    [SerializeField]
    Camera gameCam;

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

    void keyboardTest() {
        mouseClickCommandAntMove();
        keyboardSpawnAnt();
        keyboardSpawnEnemyAnt();
        keyboardDestroyAllEnemyAnt();
        keyboardDestroyAllAnt();
    }

    void keyboardSpawnAnt() {
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
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(1, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
    }
    void keyboardSpawnEnemyAnt() {
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
                gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(21, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
    }
    void keyboardDestroyAllEnemyAnt() {
        if (Input.GetKeyDown(KeyCode.G)) {
            for (int i = 0; i < gameModel.instance.ant_enemyList.Count; i++) {
                Destroy(gameModel.instance.ant_enemyList[ i ].gameObject);
            }
        }
    }
    void keyboardDestroyAllAnt() {
        if (Input.GetKeyDown(KeyCode.B)) {
            for (int i = 0; i < gameModel.instance.antList.Count; i++) {
                Destroy(gameModel.instance.antList[ i ].gameObject);
            }
        }
    }


    void mouseClickCommandAntMove() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2Int mousePos = gameModel.instance.worldToMapV3(gameCam.ScreenToWorldPoint(Input.mousePosition));
            print(mousePos);
            foreach (var item in gameModel.instance.antList) {
                item.cutOffCurMovement();
                item.Destination = mousePos;
                item.startLerpToDestination();
            }
        }
    }


    void ToUpdate() {
        autoSpawn();
        keyboardTest();
        
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
    }

    int spawnNumber = 15;
    void autoSpawn() {
        timer1 += globalVarManager.deltaTime;
        timer2 += globalVarManager.deltaTime;
        float max1 = (gameModel.instance.ant_enemyList.Count + 1) /( gameModel.instance.antList.Count + 1) ;
        float max2 = ( gameModel.instance.antList.Count + 1) / (gameModel.instance.ant_enemyList.Count + 1) ;
        if (timer1 - 0.5f > 0) {
            timer1 = 0;
            for (int i = 0; i < spawnNumber * max1; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(1, 0, 0), Quaternion.identity).GetComponent<Ant>());
                gameModel.instance.delayerValUpdate();
            }
         
        }
        if (timer2 - 0.5f > 0) {
            timer2 = 0;
            for (int i = 0; i < spawnNumber * max2; i++) {
                gameModel.instance.ant_enemyList.Add(Instantiate(ant_enemyPrefabs, new Vector3(21, 0, 0), Quaternion.identity).GetComponent<Ant>());
                gameModel.instance.delayerValUpdate();
            }
   
        }

    }

}
