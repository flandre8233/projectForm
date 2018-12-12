using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ant : MonoBehaviour {



    Vector3 pos;
    public float speed = 2.0f;

    Rigidbody2D rigidbody2D;

    public List<Vector3Int> pathfindedList;

    vector3Lerp vector3Lerp = new vector3Lerp();

    // Use this for initialization
    void Start () {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
        pos = transform.position; // Take the current position
        rigidbody2D = GetComponent<Rigidbody2D>();

        findNewPath();
        StartCoroutine(enumerator());
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
            transform.position = vector3Lerp.update();
        }
    }

    public void startLerpToDestination() {
        vector3Lerp.startLerp(transform.position, gameModel.instance.mapV3ToWorldPos(pathfindedList[ 0 ] ) , 0.3f , null, onArrivalsDestination);
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
        pathfindedList.Clear();

        pathfindedList = pathfinding.StartBakeAllFloorToVector3Int(gameModel.instance.charWorldToMapV3(transform), getNextMoveableDestination());
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

        yield return new WaitForSeconds(Random.Range(1,2) );

        startLerpToDestination();
    }

    void basedMovement() {
        if (Input.GetKey(KeyCode.A)) {           //(-1,0)
            pos += Vector3.left * 1 * Time.deltaTime;// Add -1 to pos.x
        }
        if (Input.GetKey(KeyCode.D)) {           //(1,0)
            pos += Vector3.right * 1 * Time.deltaTime;// Add 1 to pos.x
        }
        if (Input.GetKey(KeyCode.W)) {           //(0,1)
            pos += Vector3.up * 1 * Time.deltaTime; // Add 1 to pos.y
        }
        if (Input.GetKey(KeyCode.S)) {           //(0,-1)
            pos += Vector3.down * 1 * Time.deltaTime;// Add -1 to pos.y
        }
        rigidbody2D.position = Vector3.MoveTowards(transform.position, pos, speed * Time.deltaTime);    // Move there
    }

}
