using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerAnt : Ant {

    [SerializeField]
    int inventory;

    [SerializeField]
    int resourceCollectionAbility;

    [ SerializeField]
    mine targetMine;



    // Use this for initialization
    public override void Start() {
        base.Start();
    }

    public override int getRefundNum() {
        return base.getRefundNum()+inventory;
    }

    void updateWorkJob() {
        if (antActivity == AntActivityState.ChasingEnemy || antMiningActivity == AntMiningActivityState.goingToMine || antMiningActivity == AntMiningActivityState.returnToBase || !isFriendly || inventory > 0) {
            return;
        }

        if (!targetMine) {
            targetMine = gameModel.instance.getSingleMineInRange(InMapV3Pos, 3);
        }


        if (targetMine) {
                if (antActivity == AntActivityState.WalkingAround) {
                    //需要為目前地板留多一個足跡
                    pathRecord.path.Add(InMapV3Pos);
                }
                antActivity = AntActivityState.miningResource;
                antMiningActivity = AntMiningActivityState.goingToMine;
                setDestinationToMine();

        } else {
            //沒有礦物，又到達終點時
            if (pathCounter >= pathRecord.path.Count - 1 ) {
                targetMine = null;
                antActivity = AntActivityState.WalkingAround;
                antMiningActivity = AntMiningActivityState.none;
                GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
    }

    void recordPath() {
        switch (antMiningActivity) {
            case AntMiningActivityState.none:
                //如果根本很接近基地，就直接清空數據，當成重新在基地出發
                if (gameModel.instance.checkIsThereAPointNearby(InMapV3Pos, motherBase.instance.InMapV3Pos, 2)) {
                    pathRecord = new WalkingPath();
                    pathRecord.path.Add(motherBase.instance.InMapV3Pos);
                }
                //記錄舊路徑
                pathRecord.path.Add(InMapV3Pos);
                break;
            default:
                break;
        }
    }

    void collectionResourceFromMine() {
        if (antMiningActivity == AntMiningActivityState.goingToMine) {
            inAttackRange = gameModel.instance.Vector2IntEquality(targetMine.InMapV3Pos, InMapV3Pos);
            if (inAttackRange) {
                //採集
                if (targetMine && inventory <= 0) {
                    inventory += targetMine.OnBeMining(resourceCollectionAbility);

                    //先設回記憶路徑最末端
                    pathCounter = pathRecord.path.Count - 1;
                    pathRecord.serialNumber = targetMine.UID;
                    returnBaseByRecordPath();


                    antMiningActivity = AntMiningActivityState.returnToBase;
                    inAttackRange = false;
                    targetMine = null;

                    //setDestinationToHeart();
                    startLerpToDestination();
                    GetComponent<SpriteRenderer>().color = Color.green;
                } else {
                    //根本沒有礦物
                    inAttackRange = false;
                    targetMine = null;
                    antMiningActivity = AntMiningActivityState.returnToBase;
                    GetComponent<SpriteRenderer>().color = Color.gray;
                    pathCounter = pathRecord.path.Count-1;
                
                }
            }
        }
    }

    void PlacingResourcesToMotherBase() {
        //暫時
        if (antMiningActivity == AntMiningActivityState.returnToBase) {
            inAttackRange = gameModel.instance.Vector2IntEquality(motherBase.instance.InMapV3Pos, InMapV3Pos);
            if (inAttackRange) {
                    if (inventory > 0) {
                        //礦未用完就跟motherBase新路徑內容
                        motherBase.instance.addNewMinePath(pathRecord);
                    }
                gameModel.instance.resource += inventory;
                inventory = 0;

                //清空記憶
                pathRecord = new WalkingPath();

                inAttackRange = false;
                antMiningActivity = AntMiningActivityState.none;
                antActivity = AntActivityState.WalkingAround;
                resetActivityToNormal();

                GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
    }

    void CompareOthersMiningPathAndChooseBetterOne() {
        if (antMiningActivity == AntMiningActivityState.returnToBase) {
            floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);

            for (int i = 0; i < curFloorData.ants_FollowMinePath.Count; i++) {
                MinerAnt item = curFloorData.ants_FollowMinePath[ i ].gameObject.GetComponent<MinerAnt>();
                WalkingPath itemWalkingPath = item.pathRecord;
                if (item.antMiningActivity != AntMiningActivityState.followTheMinePath) {
                    continue;
                }
                if (item.pathRecord.serialNumber != pathRecord.serialNumber) {
                    continue;
                }
                
                if ( (itemWalkingPath.path.Count) > (pathRecord.path.Count)) {
                    //如果比較目標的掘礦路徑更為長 就跟他說有個更好的
                    TranPathInfo(item);
                }

            }
        }
    }

    void TranPathInfo(MinerAnt target) {
        //給他全新記錄
        target.pathRecord = new WalkingPath(pathRecord);
        target.pathCounter = pathCounter;
        target.Destination = pathRecord.path[ pathCounter ];
        target.startLerpToDestination();
    }

        void tellOtherMinerAntIfHaveMine() {
        //告知路過的其他螞蟻有好東西
        if (antMiningActivity == AntMiningActivityState.returnToBase) {

            floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
            if (inventory > 0) {
                for (int i = 0; i < curFloorData.ants_RandWalk.Count; i++) {
                    MinerAnt item = curFloorData.ants_RandWalk[ i ].gameObject.GetComponent<MinerAnt>();
                    //如果目標Ant不是礦工單位就不告訴他
                    if (item == null) {
                        continue;
                    }

                    if (item.resistOrder || !item.acceptOrderProbabilityDetermination()) {
                        
                        continue;
                    }
                        //如果有其他螞蟻無所事事
                        //就叫他去掘礦
                        item.antActivity = AntActivityState.miningResource;
                        item.antMiningActivity = AntMiningActivityState.followTheMinePath;
                        item.GetComponent<SpriteRenderer>().color = Color.yellow;

                        //給他全新記錄
                        TranPathInfo(item);
                        //startLerpToDestination();
                }
            } else {
                for (int i = 0; i < curFloorData.ants_FollowMinePath.Count; i++) {
                    MinerAnt item = curFloorData.ants_FollowMinePath[ i ].gameObject.GetComponent<MinerAnt>();
                    //如果目標Ant不是礦工單位就不告訴他
                    if (item == null) {
                        continue;
                    }
                    if (item.pathRecord.serialNumber != pathRecord.serialNumber) {
                        continue;
                    }
                    if (item.antMiningActivity != AntMiningActivityState.followTheMinePath) {
                        continue;
                    }
                    item.antMiningActivity = AntMiningActivityState.returnToBase;
                    item.GetComponent<SpriteRenderer>().color = Color.gray;

                    //給他全新記錄
                    item.pathRecord = new WalkingPath(pathRecord);
                    item.pathCounter = pathCounter;
                    item.Destination = pathRecord.path[ pathCounter ];

                    startLerpToDestination();
                }
            }

        }
    }

    public void setDestinationToMine() {
        Destination = targetMine.InMapV3Pos;
    }

    public override void onArrivalsDestination() {
        recordPath();

        //更新單位資料
        updateObjectMapInformation();

        updateWorkJob();

        //leaveSomeSmell();

        collectionResourceFromMine();

        PlacingResourcesToMotherBase();

        chooseNextDestinationAndPath();


        tellOtherMinerAntIfHaveMine();

        CompareOthersMiningPathAndChooseBetterOne();
    }

}
