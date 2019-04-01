using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WalkingPath {
    public List<Vector2Int> path;
    public int serialNumber;

    public WalkingPath(List<Vector2Int> ListV2) {
        deepCopyWriteIn(ListV2);
    }
    public WalkingPath(WalkingPath WP) {
        deepCopyWriteIn(WP.path);
        serialNumber = WP.serialNumber;
    }
    public WalkingPath() {
        path = new List<Vector2Int>();
        serialNumber = 0;
    }

    public List<Vector2Int> deepCopyOutputV2() {
        List<Vector2Int> outputData = new List<Vector2Int>();
        outputData.AddRange(path);
        return outputData;
    }
    public WalkingPath deepCopyOutputWP() {
        List<Vector2Int> newV2List = new List<Vector2Int>();
        newV2List.AddRange(path);
        WalkingPath outputData = new WalkingPath(newV2List);
        outputData.serialNumber = serialNumber;
        return outputData;
    }

public void deepCopyWriteIn(List<Vector2Int> ListV2) {
        path = new List<Vector2Int>();
        path.AddRange(ListV2);
    }

}

public class gameModel : SingletonMonoBehavior<gameModel> {
    public List<Ant> antList;
    public List<Ant> ant_enemyList;

    public List<mine> mineList;

    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    Tilemap tilemapWall;
    [SerializeField]
    static Tilemap tilemapWall2;
    [SerializeField]
    GridLayout gridLayout;

    [SerializeField]
    floorData[,] floorDatas;

    public int delayer;

    int maxFloorLength = 300;

    public int mapRadius;
    public int mapBuildRadius {
        get {
            return (int)((mapRadius) * 0.2f);
        }
    }

    public int unitLimit;

    //public Vector2Int dungeonHeartV2Point = new Vector2Int(10, -1);

    public int resource;

    //test用
    public Vector2Int minePoint;

    int UIDApplication;

    public int UIDRequest() {
        return UIDApplication++;
    }

    public int getMaxFloorLength() {
        return maxFloorLength;
    }

    public void init () {
        floorDatas = new floorData[ maxFloorLength, maxFloorLength ];
        for (int y = 0; y < maxFloorLength; y++) {
            for (int x = 0; x < maxFloorLength; x++) {
                floorDatas[ x, y ] = new floorData();
            }
        }

        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
    }

    // Update is called once per frame
    void ToUpdate() {
        smellDissipate();
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
    }

    public floorData getFloorDatas(Vector2Int pos) {
        try {
            return floorDatas[ pos.x + (maxFloorLength/2), pos.y + (maxFloorLength / 2) ];
        } catch (System.IndexOutOfRangeException) {
            Debug.LogWarning(pos );
        }
        return floorDatas[ pos.x + (maxFloorLength / 2), pos.y + (maxFloorLength / 2) ];
    }

    public void smellDissipate() {
        //只更新要更新的floorSmell
        for (int y = 0; y < maxFloorLength; y++) {
            for (int x = 0; x < maxFloorLength ; x++) {
                floorSmell curFloorSmell = floorDatas[ x, y ].floorSmell;
                if (curFloorSmell.attackSmell.smell > 0) {
                    curFloorSmell.attackSmell.smell -= globalVarManager.deltaTime;
                }
                if (curFloorSmell.enemySmell.smell > 0) {
                    curFloorSmell.enemySmell.smell -= globalVarManager.deltaTime;
                }
                if (curFloorSmell.friendlySmell.smell > 0) {
                    curFloorSmell.friendlySmell.smell -= globalVarManager.deltaTime;
                }
                if (curFloorSmell.mineSmell.smell > 0) {
                    curFloorSmell.mineSmell.smell -= globalVarManager.deltaTime;
                }
                floorDatas[ x, y ].floorSmell = curFloorSmell;
            }
        }
      
    }

    public bool tryDoSubtractBy(ref int orlVal , int SubtractVal) {
        if (orlVal - SubtractVal < 0 ) {
            operateFailEvent.instance.tryDoInvoke(operateFailEvent.instance.OnNoResourceAble);
            return false;
        }
        orlVal -= SubtractVal;
        return true;
    }

    public Vector3 mapV3ToWorldPos(Vector2Int pos) {
        Vector3 WorldPos = gridLayout.CellToWorld(new Vector3Int(pos.x,pos.y,0));
        WorldPos.x += 0.5f;
        WorldPos.y += 0.5f;
        return WorldPos;
    }

    public Vector2Int worldToMapV3(Vector3 pos) {
        Vector3Int v3 = gridLayout.WorldToCell(pos);
        return new Vector2Int(v3.x, v3.y);
    }
    public Vector2Int charWorldToMapV3(Transform Ts) {
        return worldToMapV3(Ts.position);
    }

    public Vector2Int genRandomMapV3() {
        Vector2Int res = new Vector2Int();
        res.x = Random.Range(1,21);
        res.y = Random.Range(-9,7);
        return res;
    }
    
    public float twoPointAngles(Vector3 p1,Vector3 p2) {
        float angleDeg = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;
        return angleDeg;
    }

    Ant pickUpAntFromArray(List<Ant> ants) {
        int count = ants.Count;
        if (count <= 0) {
            return null;
        }
        return ants[ Random.Range(0, count - 1) ];
    }

    public Ant checkAntInThisWall(Vector2Int mapV3) {
        floorData floorData = getFloorDatas(mapV3);
        List<Ant> ants = floorData.ants;
        return pickUpAntFromArray(ants);
    }

    public Ant checkAnt_EnemyInThisWall(Vector2Int mapV3) {
        floorData floorData = getFloorDatas(mapV3);
        List<Ant> ants = floorData.enemyAnts;
        return pickUpAntFromArray(ants);
    }

    public Ant getSingleAnt_EnemyInRange(Vector2Int pos, float range) {
        int R = (int)range;
        List<Ant> ants = new List<Ant>();
        for (int x = -R; x < R; x++) {
            for (int y = -R; y < R; y++) {
                ants.AddRange(getFloorDatas(new Vector2Int(pos.x+x,pos.y+y) ).enemyAnts );
            }
        }
        return pickUpAntFromArray(ants);
    }

    public Ant getSingleAntInRange(Vector2Int pos, float range) {
        int R = (int)range;
        List<Ant> ants = new List<Ant>();
        for (int x = -R; x < R; x++) {
            for (int y = -R; y < R; y++) {
                ants.AddRange(getFloorDatas(new Vector2Int(pos.x + x, pos.y + y)).ants);
            }
        }
        return pickUpAntFromArray(ants);
    }

    public mine getSingleMineInRange(Vector2Int pos,float range) {
        int R = (int)range;
        for (int i = 0; i < mineList.Count; i++) {
            mine item = mineList[ i ];
            Vector2Int itemV2Int = item.InMapV3Pos;
            if (pos.x + R > itemV2Int.x && pos.x - R < itemV2Int.x &&
                pos.y + R > itemV2Int.y && pos.y - R < itemV2Int.y
                ) {
                return item;
            }
        }

        //cant find
        return null;
    }

    public bool Vector2IntEquality(Vector2Int v2_1,Vector2Int v2_2) {
        if (v2_1.x != v2_2.x || v2_1.y != v2_2.y) {
            return false;
        }
        return true;
    }

    public List<Ant> getAntListInRange(Vector2Int pos,float range) {
        List<Ant> result = new List<Ant>();
        int R = (int)range;
        List<Ant> ants = new List<Ant>();
        for (int x = -R; x < R; x++) {
            for (int y = -R; y < R; y++) {
                ants.AddRange(getFloorDatas(new Vector2Int(pos.x + x, pos.y + y)).ants);
            }
        }

        int count = ants.Count;
        for (int i = 0; i < count; i++) {
            Ant item = ants[ i ];
            result.Add(item);
        }
        return result;
    }

    public bool checkIsThereAPointNearby(Vector2Int orlPoint,Vector2Int targetPoint,int radius) {
        if (orlPoint.x + radius < targetPoint.x || orlPoint.x - radius >  targetPoint.x || orlPoint.y + radius < targetPoint.y || orlPoint.y - radius > targetPoint.y) {
            return false;
        }
        return true;
    }

    public bool checkNextToIsWall(Vector2Int mapV3) {
        Vector2Int[] nexttoList = new Vector2Int[ 4 ];
        nexttoList[ 0 ] = mapV3;
        nexttoList[ 1 ] = mapV3;
        nexttoList[ 2 ] = mapV3;
        nexttoList[ 3 ] = mapV3;

        nexttoList[ 0 ].x += 1;
        nexttoList[ 1 ].x -= 1;
        nexttoList[ 2 ].y += 1;
        nexttoList[ 3 ].y -= 1;

        for (int i = 0; i < nexttoList.Length; i++) {
            Vector2Int item = nexttoList[ i ];
            if (checkThisVectorIntIsWall(item)) {
                return true;
            }
        }
        return false;
    }

    public bool checkThisVectorIntIsWall(Vector2Int item) {
        if (tilemapWall.GetTile(new Vector3Int(item.x, item.y,0))) {
            return true;
        }
        return false;
    }

    public bool successRateCalculation(float successRate) {
        return Random.Range(0, 100) < successRate ? true : false;
    }

    public bool checkUnitLimit(int newUnit) {
       if( antList.Count + newUnit <= unitLimit ) {
            return true;
        }
        operateFailEvent.instance.tryDoInvoke(operateFailEvent.instance.OnLimitFull);
        return false;
    }

    public void delayerValUpdate() {
        int numberOfAnt = antList.Count + ant_enemyList.Count;

        delayer = numberOfAnt / 300;
    }

    public Vector2Int getSingleLineRandom(int singleLineArea) {
        if (Random.Range(0,1) <= 0) {
            singleLineArea *= -1;
        } 
        float randomAngle = Random.Range(0, 360);
        Vector2Int randomMapv3 = polarCoordinates(motherBase.instance.InMapV3Pos, randomAngle, (int)(getLengthForDeg(randomAngle) * singleLineArea));
        return randomMapv3;
    }

    public Vector2Int getRandomPoint() {
        return getSingleLineRandom(Random.Range(0, mapRadius)); ;
    }

    float getLengthForDeg(float angle) {
        angle = ((angle + 45) % 90 - 45) / 180 * Mathf.PI;
        return 1 / Mathf.Cos(angle);
    }

    public Vector2Int polarCoordinates(Vector2Int orl_point, float angle, int dist) {
        float x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
        Vector2 newPosition = orl_point;
        newPosition.x += x;
        newPosition.y += y;
        return Vector2Int.CeilToInt(newPosition);
    }
    public Vector2Int polarCoordinatesButSquare(Vector2Int orl_point, float angle, int dist) {
        float radius = dist * Mathf.Cos(Mathf.PI / 4);
        float x = orl_point.x + radius * Mathf.Cos(angle);
        float y = orl_point.y + radius * Mathf.Sin(angle);
        return Vector2Int.CeilToInt(new Vector2(x, y));
    }

}
