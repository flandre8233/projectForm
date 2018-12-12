using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class gameModel : SingletonMonoBehavior<gameModel> {
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    Tilemap tilemapWall;
    [SerializeField]
    static Tilemap tilemapWall2;
    [SerializeField]
    GridLayout gridLayout;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    public Vector3 mapV3ToWorldPos(Vector3Int pos) {
        Vector3 WorldPos = gridLayout.CellToWorld(pos);
        WorldPos.x += 0.5f;
        WorldPos.y += 0.5f;
        return WorldPos;

    }

    public Vector3Int worldToMapV3(Vector3 pos) {
        return gridLayout.WorldToCell(pos);
    }
    public Vector3Int charWorldToMapV3(Transform Ts) {
        return gridLayout.WorldToCell(Ts.position);
    }

    public Vector3Int genRandomMapV3() {
        Vector3Int res = new Vector3Int();
        res.x = Random.Range(1,20);
        res.y = Random.Range(-7,6);
        return res;
    }

    public bool checkNextToIsWall(Vector3Int mapV3) {
        Vector3Int[] nexttoList = new Vector3Int[ 4 ];
        nexttoList[ 0 ] = mapV3;
        nexttoList[ 1 ] = mapV3;
        nexttoList[ 2 ] = mapV3;
        nexttoList[ 3 ] = mapV3;

        nexttoList[ 0 ].x += 1;
        nexttoList[ 1 ].x -= 1;
        nexttoList[ 2 ].y += 1;
        nexttoList[ 3 ].y -= 1;

        for (int i = 0; i < nexttoList.Length; i++) {
            Vector3Int item = nexttoList[ i ];
            if (checkThisVectorIntIsWall(item)) {
                return true;
            }
        }
        return false;
    }

    public bool checkThisVectorIntIsWall(Vector3Int item) {
        if (tilemapWall.GetTile(item)) {
            return true;
        }
        return false;
    }

}
