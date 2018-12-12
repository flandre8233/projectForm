using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ant : MonoBehaviour {
    Vector3 pos;
    public float speed = 2.0f;

    [SerializeField]
    public List<Vector3Int> pathfindedListInt;
    public List<Vector3> pathfindedList;

    vector3Lerp vector3Lerp = new vector3Lerp();
    

    // Use this for initialization
    void Start () {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
        pos = transform.position; // Take the current position

        findNewPath();
        startLerpToDestination();
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
    }

    // Update is called once per frame
    void ToUpdate () {
        //basedMovement();

        //print(res);
        //Vector3Int res = gameModel.instance.getCurCharMapV3(transform);
        //gameModel.instance.checkNextToIsWall(res);

        toDestination();
    }

    bool onDestination;

    void toDestination() {
        if (vector3Lerp.isLerping) {
            Vector3 toTargetVector = pathfindedList[ 0 ] - transform.position;
            float zRotation = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, zRotation+-90));

            transform.position = vector3Lerp.update();

         
        }
    }

    public void startLerpToDestination() {
        vector3Lerp.startLerp(transform.position, pathfindedList[ 0 ]  , 0.3f , null, onArrivalsDestination);

        //面向角度
    }

    void onArrivalsDestination() {
        //已現到達目標0，所以把目標0除掉
        pathfindedList.RemoveAt(0);
        if (pathfindedList.Count>0) {
            //如果還有目的地就繼續走動
            startLerpToDestination();
        } else {
            //所有目的地已經到達

            findNewPath();
            StartCoroutine(enumerator());
        }
    }

    void findNewPath() {
        pathfindedListInt.Clear();
        pathfindedList.Clear();

        pathfindedListInt = pathfinding.StartBakeAllFloorToVector3Int(gameModel.instance.charWorldToMapV3(transform), getNextMoveableDestination());
        //轉為世界坐標
        for (int i = 0; i < pathfindedListInt.Count; i++) {
            pathfindedList.Add(gameModel.instance.mapV3ToWorldPos(pathfindedListInt[ i]));
        }
        //將最後目的地變得有點亂數
        Vector3 lastOneV3 = pathfindedList[ pathfindedList.Count - 1 ];
        lastOneV3.x = pathfindedList[ pathfindedList.Count - 1 ].x + Random.Range(-0.5f,0.5f);
        lastOneV3.y = pathfindedList[ pathfindedList.Count - 1 ].y + Random.Range(-0.5f, 0.5f);
        pathfindedList[ pathfindedList.Count - 1 ] = lastOneV3;


        if (pathfindedList.Count <= 0) {
            findNewPath();
        }
    }

    Vector3Int getNextMoveableDestination() {
        Vector3Int randomMapv3 = gameModel.instance.genRandomMapV3();
        if (gameModel.instance.checkThisVectorIntIsWall(randomMapv3)) {
            getNextMoveableDestination();
        }
        return randomMapv3;
    }

    IEnumerator enumerator() {

        yield return new WaitForSeconds(Random.Range(7 + gameModel.instance.delayer, 15 + gameModel.instance.delayer) );

        startLerpToDestination();
    }

}
