using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : SingletonMonoBehavior<gameController> {

    [SerializeField]
    Camera gameCam;

    [SerializeField]
    GameObject antPrefabs;
    [SerializeField]
    GameObject antMinerPrefabs;
    [SerializeField]
    GameObject ant_enemyPrefabs;
    [SerializeField]
    GameObject ant_enemyPrefabs_big;
    [SerializeField]
    GameObject MineBuilding;

    float timer1;
    float timer2;

    //暫放在這裡而已
    public void spawnMineBuilding(Vector2Int MapPos , int startResource ) {
        if (startResource <= 0) {
            return;
        }

        mine item = Instantiate(MineBuilding).GetComponent<mine>();
        item.init(MapPos, startResource);
    }

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

        
        GameObject[] MineGameObject = GameObject.FindGameObjectsWithTag("Mine");
        foreach (var item in MineGameObject) {
            mine itemMine = item.GetComponent<mine>();
            itemMine.init(itemMine.InMapV3Pos);
        }
        GameObject[] MotherBaseGameObject = GameObject.FindGameObjectsWithTag("MotherBase");
        foreach (var item in MotherBaseGameObject) {
            motherBase itemMotherBase = item.GetComponent<motherBase>();
            itemMotherBase.init(itemMotherBase.InMapV3Pos);
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

        CameraHandler.instance.init();

        for (int i = 0; i < 1000; i++) {
            Ant item = Instantiate(antPrefabs, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Ant>();
            gameModel.instance.antList.Add(item);
            item.addCost(antTypeCost[ 1 ]);
        }
        gameModel.instance.delayerValUpdate();

        for (int i = 0; i < 2000; i++) {
            Ant item = Instantiate(antMinerPrefabs, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Ant>();
            gameModel.instance.antList.Add(item);
            item.addCost(antTypeCost[ 0 ]);
        }
        gameModel.instance.delayerValUpdate();

    }

    void keyboardTest() {
        //mouseClickCommandAntMove();
        keyboardSpawnAnt();
        keyboardSpawnEnemyAntFromMapEdge();
        mouseClickSelectAnt();
        //keyboardSpawnEnemyAnt();
        //keyboardDestroyAllEnemyAnt();
        //keyboardDestroyAllAnt();
    }

    [SerializeField]
    int[] antTypeCost;

    void keyboardSpawnAnt() {

        if (Input.GetKeyDown(KeyCode.Z) && gameModel.instance.checkUnitLimit(1) && gameModel.instance.tryDoSubtractBy(ref gameModel.instance.resource, antTypeCost[0]) ) {
            Ant item = Instantiate(antMinerPrefabs, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Ant>();
            gameModel.instance.antList.Add(item);
            item.addCost(antTypeCost[ 0 ]);

            gameModel.instance.delayerValUpdate();
        }



        /*
        if (Input.GetKeyDown(KeyCode.X)) {
            for (int i = 0; i < 10; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            for (int i = 0; i < 100; i++) {
                gameModel.instance.antList.Add(Instantiate(antPrefabs, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();

        }
        if (Input.GetKeyDown(KeyCode.V)) {
            for (int i = 0; i < 1000; i++) {
                gameModel.instance.antList.Add(Instantiate(antMinerPrefabs, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Ant>());
            }
            gameModel.instance.delayerValUpdate();
        }
        */
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

    void keyboardSpawnEnemyAntFromMapEdge() {
        if (Input.GetKeyDown(KeyCode.A)) {
            for (int i = 0; i < 100; i++) {
                Ant item = Instantiate(ant_enemyPrefabs, gameModel.instance.mapV3ToWorldPos(gameModel.instance.getSingleLineRandom(gameModel.instance.mapRadius + 2)), Quaternion.identity).GetComponent<Ant>();
                gameModel.instance.ant_enemyList.Add(item);
                item.setDestinationToHeart();

                gameModel.instance.delayerValUpdate();
            }
    
        }
        // setDestinationToHeart
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

    Vector2Int getMouseMapPos() {
        Vector2Int mousePos = gameModel.instance.worldToMapV3(gameCam.ScreenToWorldPoint(Input.mousePosition));
        return mousePos;
    }

    void mouseClickCommandAntMove() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2Int mousePos = getMouseMapPos();
            foreach (var item in gameModel.instance.antList) {
                item.cutOffCurMovement();
                item.Destination = mousePos;
                item.startLerpToDestination();
            }
        }
    }

    void mouseClickSelectAnt() {
        if (!CameraHandler.instance.inPan && Input.GetMouseButtonUp(0)) {
            Vector2Int mousePos = getMouseMapPos();
            Ant ant = gameModel.instance.checkAntInThisWall(mousePos);
            if (ant) {
                ant.alreadyDead = true;
                print("お前はもう死んでいる");
            }
        }
    }


    void ToUpdate() {
        //autoSpawn();
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
