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

    static List<Vector2Int> outputData = new List<Vector2Int>();
    static List<Vector2IntPathDir>  checker = new List<Vector2IntPathDir>();
    static bool isFindEnd = false;

    //從這裡開始進行pathfinding
    public static List<Vector2Int> StartBakeAllFloorToVector2Int(Vector2Int bakeCenter, Vector2Int endPoint) {

        BakeAllFloor(bakeCenter, endPoint);
        switch (curPathfindingMethod) {
            case pathfindingMethod.Diffusion:
                return compositionRightPath(bakeCenter);
            case pathfindingMethod.straightLine:
                List<Vector2Int> finalOutputData = new List<Vector2Int>();
                for (int i = 1; i < outputData.Count; i++) {
                    finalOutputData.Add(outputData[i]);
                }
                return finalOutputData;
        }

        return compositionRightPath(bakeCenter);
    }

    static List<Vector2IntPathDir>BakeAllFloor(Vector2Int bakeCenter, Vector2Int endPoint) {
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

    public static Vector2Int getSinglePathData(Vector2Int bakeCenter, Vector2Int endPoint) {
        return straightLinePathfindingMainCode(bakeCenter,endPoint);
    }


    //遞迴
    static List<Vector2Int> DiffusionRecursive(List<Vector2Int> loopArray, Vector2Int bakeCenter, Vector2Int endPoint) {

        if (loopArray.Count <= 0) {
            Debug.Log("count = 0");
            return null;
        }

        List<Vector2Int> nextLoopGroundArray = new List<Vector2Int>();
        for (int i = 0; i < loopArray.Count; i++) {
            Vector2Int item = loopArray[ i ];
            if (gameModel.instance.Vector2IntEquality( item , new Vector2Int()) ) {
                continue;
            }
            if (isFindEnd) {
                break;
            }
            nextLoopGroundArray.AddRange(getNeighbor(item, bakeCenter, endPoint));

        }

        return DiffusionRecursive(nextLoopGroundArray, bakeCenter, endPoint);
    }

    static void straightRecursive(Vector2Int objectPos, Vector2Int endPoint) {
        outputData.Add(straightLinePathfindingMainCode (objectPos, endPoint) );
        if (!isFindEnd) {
            straightRecursive(outputData[ outputData.Count - 1 ], endPoint);
        }
    }

        static List<Vector2Int> neighborArray = new List<Vector2Int>();

    static List<Vector2Int>  getNeighbor(Vector2Int objectPos, Vector2Int bakeCenter, Vector2Int endPoint) {
        //default pathfinding main code
        return DiffusionGetNeighbor(objectPos, bakeCenter, endPoint);
    }

    static List<Vector2Int> DiffusionGetNeighbor(Vector2Int objectPos, Vector2Int bakeCenter, Vector2Int endPoint) {
        neighborArray.Clear();
        Vector2Int[] dirArray = new Vector2Int[ 4 ];
        dirArray[ 0 ] = objectPos;
        dirArray[ 1 ] = objectPos;
        dirArray[ 2 ] = objectPos;
        dirArray[ 3 ] = objectPos;

        dirArray[ 0 ].y += 1; // in
        dirArray[ 1 ].y -= 1; // out 
        dirArray[ 2 ].x -= 1; // left
        dirArray[ 3 ].x += 1; // right

        for (int i = 0; i < dirArray.Length; i++) {
            Vector2IntPathDir item = new Vector2IntPathDir();
            //this
            Vector2Int Vector2Int = dirArray[ i ];


            if (gameModel.instance.Vector2IntEquality( Vector2Int , bakeCenter) ) {
                continue;
            }

            if (InquireAlreadyCheck(Vector2Int)) {
                continue;
            }

            if (gameModel.instance.checkThisVectorIntIsWall(Vector2Int)) {
                continue;
            }



            item.Vector2Int = Vector2Int;

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

            neighborArray.Add(Vector2Int);

            if (gameModel.instance.Vector2IntEquality( Vector2Int , endPoint) ) {
                isFindEnd = true;
                break;
            }

        }
        return neighborArray;
    }

    static Vector2Int straightLinePathfindingMainCode(Vector2Int objectPos, Vector2Int endPoint) {
        //沒有牆壁碰撞系統
        neighborArray.Clear();
        Vector2Int nextFootStep = objectPos;
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

        if (gameModel.instance.Vector2IntEquality(nextFootStep , endPoint) ) {
            isFindEnd = true;
        }

        return nextFootStep;
    }

    static bool  InquireAlreadyCheck(Vector2Int Vector2Int) {
        for (int i = 0; i < checker.Count; i++) {
            Vector2IntPathDir item = checker[ i ];
            if (gameModel.instance.Vector2IntEquality(Vector2Int , item.Vector2Int) ) {
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

    static Vector2Int moveConverter(Vector2Int Int3,Direction orlDir) {
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

    static List<Vector2IntPathDir> thePathWithDir = new List<Vector2IntPathDir>();

    //將結果弄成單一路徑
    static List<Vector2Int>  compositionRightPath(Vector2Int endPoint) {
        if (!isFindEnd) {
            return new List<Vector2Int>();
        }

        Vector2IntPathDir start = checker[ checker.Count - 1 ];
        //List<Vector2IntPathDir> thePathWithDir = new List<Vector2IntPathDir>();
        thePathWithDir.Clear();
        thePathWithDir.Add(start);

        Vector2Int nextPos = moveConverter(thePathWithDir[ thePathWithDir.Count - 1 ].Vector2Int, reverser(thePathWithDir[ thePathWithDir.Count - 1 ].direction));
        //Debug.Log((string)nextPos);

        while (nextPos != endPoint) {
            Vector2IntPathDir Vector2IntPathDir = InquireAlreadyCheckItem(nextPos);
            thePathWithDir.Add(Vector2IntPathDir);

            nextPos = moveConverter(thePathWithDir[ thePathWithDir.Count - 1 ].Vector2Int, reverser(thePathWithDir[ thePathWithDir.Count - 1 ].direction));
            //Debug.Log((string)nextPos + (string)endPoint);
        }

        List<Vector2Int> realPath = new List<Vector2Int>();
        for (int i = 0; i < thePathWithDir.Count; i++) {
            Vector2IntPathDir item = thePathWithDir[ i ];
            realPath.Add(item.Vector2Int);
        }
        realPath.Add(endPoint);

        realPath.Reverse();
        return realPath;
    }

    static Vector2IntPathDir InquireAlreadyCheckItem(Vector2Int Vector2Int) {
        for (int i = 0; i < checker.Count; i++) {
            Vector2IntPathDir item = checker[ i ];
            if (gameModel.instance.Vector2IntEquality( Vector2Int , item.Vector2Int) ) {
                return item;
            }
        }
        return null;
    }

    [System.Serializable]
    public class Vector2IntPathDir {
        public Direction direction;
        public Vector2Int Vector2Int;
    }

    

}
