using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class gameModel : SingletonMonoBehavior<gameModel> {
    public List<Ant> antList;
    public List<Ant> ant_enemyList;

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

    public void init () {
        floorDatas = new floorData[ 100,100];
        for (int y = 0; y < 100; y++) {
            for (int x = 0; x < 100; x++) {
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
        return floorDatas[ pos.x + 50, pos.y + 50 ];
    }

    public void smellDissipate() {
        int minX = 1;
        int maxX = 20;
        int minY = -7;
        int maxY = 6 ;
        //只更新要更新的floorSmell
        for (int y = minY + 50; y < maxY + 50; y++) {
            for (int x = minX + 50; x < maxX + 50; x++) {
                floorSmell curFloorSmell = floorDatas[ x, y ].floorSmell;
                if (curFloorSmell.attackSmell > 0) {
                    curFloorSmell.attackSmell -= globalVarManager.deltaTime;
                }
                if (curFloorSmell.enemySmell > 0) {
                    curFloorSmell.enemySmell -= globalVarManager.deltaTime;
                }
                if (curFloorSmell.friendlySmell > 0) {
                    curFloorSmell.friendlySmell -= globalVarManager.deltaTime;
                }
                if (curFloorSmell.mineSmell > 0) {
                    curFloorSmell.mineSmell -= globalVarManager.deltaTime;
                }
                floorDatas[ x, y ].floorSmell = curFloorSmell;
            }
        }
      
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
        res.y = Random.Range(-7,7);
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

    public void delayerValUpdate() {
        int numberOfAnt = gameModel.instance.antList.Count + gameModel.instance.ant_enemyList.Count;

        delayer = numberOfAnt / 300;
    }

}
