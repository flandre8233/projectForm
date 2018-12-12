using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class pathfinding {
    public enum Direction {
        Up,
        Down,
        Left,
        Right
    }

    enum pathfindingMethod {
        Diffusion,
        straightLine
    }

    static pathfindingMethod curPathfindingMethod = pathfindingMethod.straightLine;

    static List<Vector3Int> outputData = new List<Vector3Int>();
    static List<Vector3IntPathDir>  checker = new List<Vector3IntPathDir>();
    static bool isFindEnd = false;

    //從這裡開始進行pathfinding
    public static List<Vector3Int> StartBakeAllFloorToVector3Int(Vector3Int bakeCenter, Vector3Int endPoint) {

        BakeAllFloor(bakeCenter, endPoint);
        switch (curPathfindingMethod) {
            case pathfindingMethod.Diffusion:
                return compositionRightPath(bakeCenter);
            case pathfindingMethod.straightLine:
                List<Vector3Int> finalOutputData = new List<Vector3Int>();
                for (int i = 1; i < outputData.Count; i++) {
                    finalOutputData.Add(outputData[i]);
                }
                return finalOutputData;
        }

        return compositionRightPath(bakeCenter);
    }

    static List<Vector3IntPathDir>BakeAllFloor(Vector3Int bakeCenter, Vector3Int endPoint) {
        outputData.Clear();
        checker.Clear();
        isFindEnd = false;

        outputData.Add(bakeCenter);
        switch (curPathfindingMethod) {
            case pathfindingMethod.Diffusion:
                DiffusionRecursive(outputData, bakeCenter, endPoint);
                break;
            case pathfindingMethod.straightLine:
                straightRecursive(outputData[ outputData.Count - 1 ], endPoint);
                break;
        }
        return checker;
    }



    //遞迴
    static List<Vector3Int> DiffusionRecursive(List<Vector3Int> loopArray, Vector3Int bakeCenter, Vector3Int endPoint) {

        if (loopArray.Count <= 0) {
            Debug.Log("count = 0");
            return null;
        }

        List<Vector3Int> nextLoopGroundArray = new List<Vector3Int>();
        for (int i = 0; i < loopArray.Count; i++) {
            Vector3Int item = loopArray[ i ];
            if (item == new Vector3Int()) {
                continue;
            }
            if (isFindEnd) {
                break;
            }
            nextLoopGroundArray.AddRange(getNeighbor(item, bakeCenter, endPoint));

        }

        return DiffusionRecursive(nextLoopGroundArray, bakeCenter, endPoint);
    }

    static void straightRecursive(Vector3Int objectPos, Vector3Int endPoint) {
        outputData.Add(straightLinePathfindingMainCode (objectPos, endPoint) );
        if (!isFindEnd) {
            straightRecursive(outputData[ outputData.Count - 1 ], endPoint);
        }
    }

        static List<Vector3Int> neighborArray = new List<Vector3Int>();

    static List<Vector3Int>  getNeighbor(Vector3Int objectPos, Vector3Int bakeCenter, Vector3Int endPoint) {
        //default pathfinding main code
        return DiffusionGetNeighbor(objectPos, bakeCenter, endPoint);
    }

    static List<Vector3Int> DiffusionGetNeighbor(Vector3Int objectPos, Vector3Int bakeCenter, Vector3Int endPoint) {
        neighborArray.Clear();
        Vector3Int[] dirArray = new Vector3Int[ 4 ];
        dirArray[ 0 ] = objectPos;
        dirArray[ 1 ] = objectPos;
        dirArray[ 2 ] = objectPos;
        dirArray[ 3 ] = objectPos;

        dirArray[ 0 ].y += 1; // in
        dirArray[ 1 ].y -= 1; // out 
        dirArray[ 2 ].x -= 1; // left
        dirArray[ 3 ].x += 1; // right

        for (int i = 0; i < dirArray.Length; i++) {
            Vector3IntPathDir item = new Vector3IntPathDir();
            //this
            Vector3Int Vector3Int = dirArray[ i ];


            if (Vector3Int == bakeCenter) {
                continue;
            }

            if (InquireAlreadyCheck(Vector3Int)) {
                continue;
            }

            if (gameModel.instance.checkThisVectorIntIsWall(Vector3Int)) {
                continue;
            }



            item.Vector3Int = Vector3Int;

            switch (i) {
                case 0:
                    item.direction = Direction.Up;
                    break;
                case 1:
                    item.direction = Direction.Down;
                    break;
                case 2:
                    item.direction = Direction.Left;
                    break;
                case 3:
                    item.direction = Direction.Right;
                    break;

            }

            checker.Add(item);

            neighborArray.Add(Vector3Int);

            if (Vector3Int == endPoint) {
                isFindEnd = true;
                break;
            }

        }
        return neighborArray;
    }

    static Vector3Int straightLinePathfindingMainCode(Vector3Int objectPos, Vector3Int endPoint) {
        //沒有牆壁碰撞系統
        neighborArray.Clear();
        Vector3Int nextFootStep = objectPos;
        float angleDeg = Mathf.Atan2(endPoint.y - nextFootStep.y, endPoint.x - nextFootStep.x) * 180 / Mathf.PI;
        if (angleDeg <= 45f && angleDeg > -45f) {
            //右
            nextFootStep.x += 1;
        } else if (angleDeg <= -45f && angleDeg > -135f) {
            //下
            nextFootStep.y -= 1;
        } else if (angleDeg <= -135f || angleDeg > 135f) {
            //左
            nextFootStep.x -= 1;
        } else {
            //上
            nextFootStep.y += 1;
        }
        

        if (nextFootStep == endPoint) {
            isFindEnd = true;

        }

        return nextFootStep;
    }

    static bool  InquireAlreadyCheck(Vector3Int Vector3Int) {
        for (int i = 0; i < checker.Count; i++) {
            Vector3IntPathDir item = checker[ i ];
            if (Vector3Int == item.Vector3Int) {
                return true;
            }
        }
        return false;
    }

    static Direction reverser(Direction direction) {
        switch (direction) {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
        }
        return Direction.Up;
    }

    static Vector3Int moveConverter(Vector3Int Int3,Direction orlDir) {
        switch (orlDir) {
            case Direction.Up:
                Int3.y += 1;
                break;
            case Direction.Down:
                Int3.y -= 1;
                break;
            case Direction.Left:
                Int3.x -= 1;
                break;
            case Direction.Right:
                Int3.x += 1;
                break;
        }
        return Int3;
    }

    static List<Vector3IntPathDir> thePathWithDir = new List<Vector3IntPathDir>();

    //將結果弄成單一路徑
    static List<Vector3Int>  compositionRightPath(Vector3Int endPoint) {
        if (!isFindEnd) {
            return new List<Vector3Int>();
        }

        Vector3IntPathDir start = checker[ checker.Count - 1 ];
        //List<Vector3IntPathDir> thePathWithDir = new List<Vector3IntPathDir>();
        thePathWithDir.Clear();
        thePathWithDir.Add(start);

        Vector3Int nextPos = moveConverter(thePathWithDir[ thePathWithDir.Count - 1 ].Vector3Int, reverser(thePathWithDir[ thePathWithDir.Count - 1 ].direction));
        //Debug.Log((string)nextPos);

        while (nextPos != endPoint) {
            Vector3IntPathDir Vector3IntPathDir = InquireAlreadyCheckItem(nextPos);
            thePathWithDir.Add(Vector3IntPathDir);

            nextPos = moveConverter(thePathWithDir[ thePathWithDir.Count - 1 ].Vector3Int, reverser(thePathWithDir[ thePathWithDir.Count - 1 ].direction));
            //Debug.Log((string)nextPos + (string)endPoint);
        }

        List<Vector3Int> realPath = new List<Vector3Int>();
        for (int i = 0; i < thePathWithDir.Count; i++) {
            Vector3IntPathDir item = thePathWithDir[ i ];
            realPath.Add(item.Vector3Int);
        }
        realPath.Add(endPoint);

        realPath.Reverse();
        return realPath;
    }

    static Vector3IntPathDir InquireAlreadyCheckItem(Vector3Int Vector3Int) {
        for (int i = 0; i < checker.Count; i++) {
            Vector3IntPathDir item = checker[ i ];
            if (Vector3Int == item.Vector3Int) {
                return item;
            }
        }
        return null;
    }

    [System.Serializable]
    public class Vector3IntPathDir {
        public Direction direction;
        public Vector3Int Vector3Int;
    }

    

}
