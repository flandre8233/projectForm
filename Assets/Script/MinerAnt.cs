using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerAnt : Ant
{
    bool inCallback;

    public void OnCallBack()
    {
        inCallback = true;

        pathCounter = pathRecord.path.Count - 1;
        returnBaseByRecordPath();

        antMiningActivity = AntMiningActivityState.returnToBase;
        inAttackRange = false;
        targetMine = null;

        startLerpToDestination();
    }

    public void OnResetCallBack()
    {
        inCallback = false;
        if (inventory > 0)
        {
            return;
        }
        else
        {
            antMiningActivity = AntMiningActivityState.none;
            antActivity = AntActivityState.WalkingAround;
            resetActivityToNormal();
        }
    }

    [SerializeField]
    int inventory;

    [SerializeField]
    int resourceCollectionAbility;

    [SerializeField]
    int miningNeedTime;

    [SerializeField]
    mine targetMine;

    bool FollowMinePathSmellRecord = false;

    int carryMax;

    public enum AntMiningActivityState
    {
        none,
        returnToBase,
        goingToMine,
        followTheMinePath,
        mining
    }

    [SerializeField]
    AntMiningActivityState _antMiningActivity;

    public AntMiningActivityState antMiningActivity {
        get {
            return _antMiningActivity;
        }
        set {
            _antMiningActivity = value;
            if (value != AntMiningActivityState.none)
            {
                if (randWalkSmellRecord)
                {
                    gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterRandWalkAntData(this);
                    randWalkSmellRecord = false;
                }
            }
        }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (FollowMinePathSmellRecord)
        {
            gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterFollowMinePathAntData(this);
            FollowMinePathSmellRecord = false;
        }
    }

    public override int getRefundNum()
    {
        return base.getRefundNum() + inventory;
    }

    public void OnStartMining()
    {
        antMiningActivity = AntMiningActivityState.mining;
        //globalUpdateManager.instance.registerUpdateDg(miningMethod);
        globalUpdateManager.instance.startGlobalTimer(miningNeedTime, OnEndMining);
    }

    public void OnEndMining()
    {
        if (!this)
        {
            return;
        }

        //採集
        if (targetMine && inventory <= 0)
        {
            inventory += targetMine.OnBeMining(resourceCollectionAbility);

            //先設回記憶路徑最末端
            pathCounter = pathRecord.path.Count - 1;
            pathRecord.serialNumber = targetMine.UID;
            returnBaseByRecordPath();


            antMiningActivity = AntMiningActivityState.returnToBase;
            inAttackRange = false;
            targetMine = null;

            //setDestinationToHeart();
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            //根本沒有礦物
            inAttackRange = false;
            targetMine = null;
            antMiningActivity = AntMiningActivityState.returnToBase;
            GetComponent<SpriteRenderer>().color = Color.gray;
            pathCounter = pathRecord.path.Count - 1;

        }

        startLerpToDestination();

    }


    void updateWorkJob()
    {
        if (antActivity == AntActivityState.ChasingEnemy || antMiningActivity == AntMiningActivityState.goingToMine || antMiningActivity == AntMiningActivityState.returnToBase || !isFriendly || inventory > 0)
        {
            return;
        }

        if (!targetMine)
        {
            targetMine = gameModel.instance.getSingleMineInRange(InMapV3Pos, 3);
        }


        if (targetMine)
        {
            if (antActivity == AntActivityState.WalkingAround)
            {
                //需要為目前地板留多一個足跡
                pathRecord.path.Add(InMapV3Pos);
            }
            antActivity = AntActivityState.miningResource;
            antMiningActivity = AntMiningActivityState.goingToMine;
            setDestinationToMine();

        }


        else
        {
            //沒有礦物，又到達終點時
            if (pathCounter >= pathRecord.path.Count - 1)
            {
                targetMine = null;
                antActivity = AntActivityState.WalkingAround;
                antMiningActivity = AntMiningActivityState.none;
                GetComponent<SpriteRenderer>().color = normalStateColor;
            }
        }



    }

    void recordPath()
    {
        switch (antMiningActivity)
        {
            case AntMiningActivityState.none:
                //如果根本很接近基地，就直接清空數據，當成重新在基地出發
                if (gameModel.instance.checkIsThereAPointNearby(InMapV3Pos, motherBase.instance.InMapV3Pos, 2))
                {
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

    void collectionResourceFromMine()
    {
        if (antMiningActivity == AntMiningActivityState.goingToMine)
        {
            inAttackRange = gameModel.instance.Vector2IntEquality(targetMine.InMapV3Pos, InMapV3Pos);
            if (inAttackRange)
            {
                //採集
                if (targetMine && inventory <= 0)
                {
                    OnStartMining();
                }
                else
                {
                    //根本沒有礦物
                    inAttackRange = false;
                    targetMine = null;
                    antMiningActivity = AntMiningActivityState.returnToBase;
                    GetComponent<SpriteRenderer>().color = Color.gray;
                    pathCounter = pathRecord.path.Count - 1;

                }
            }
        }
    }

    void PlacingResourcesToMotherBase()
    {
        //暫時
        if (antMiningActivity == AntMiningActivityState.returnToBase)
        {
            inAttackRange = gameModel.instance.Vector2IntEquality(motherBase.instance.InMapV3Pos, InMapV3Pos);
            if (inAttackRange)
            {
                if (inventory > 0)
                {
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

                GetComponent<SpriteRenderer>().color = normalStateColor;
            }
        }
    }

    void CompareOthersMiningPathAndChooseBetterOne()
    {

        if (antMiningActivity != AntMiningActivityState.none)
        {
            return;
        }

        // if (antMiningActivity == AntMiningActivityState.returnToBase) {
        floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);

        //ants_FollowMinePath
        for (int i = 0; i < curFloorData.ants.Count; i++)
        {
            MinerAnt item = curFloorData.ants[i].gameObject.GetComponent<MinerAnt>();
            if (!item)
            {
                break;
            }

            WalkingPath itemWalkingPath = item.pathRecord;
            if (itemWalkingPath.path.Count <= 0)
            {
                break;
            }
            /*
            if (item.antMiningActivity != AntMiningActivityState.followTheMinePath) {
                continue;
            }
            if (item.pathRecord.serialNumber != pathRecord.serialNumber) {
                continue;
            }
            */

            if ((itemWalkingPath.path.Count) < (pathRecord.path.Count))
            {
                //如果比較目標的掘礦路徑更為短 就偷他的
                TranPathInfo(item);
            }

        }
        // }
    }

    void TranPathInfo(MinerAnt target)
    {
        //給自己全新記錄
        //  target.pathRecord = new WalkingPath(pathRecord);
        pathRecord = new WalkingPath(target.pathRecord);
        //target.pathCounter = pathCounter;
        pathCounter = pathRecord.path.Count - 1;
        //target.Destination = pathRecord.path[ pathCounter ];
        // target.startLerpToDestination();
    }

    void tellOtherMinerAntIfHaveMine()
    {
        //告知路過的其他螞蟻有好東西
        if (antMiningActivity == AntMiningActivityState.returnToBase)
        {

            floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
            if (inventory > 0)
            {
                for (int i = 0; i < curFloorData.ants_RandWalk.Count; i++)
                {
                    MinerAnt item = curFloorData.ants_RandWalk[i].gameObject.GetComponent<MinerAnt>();
                    //如果目標Ant不是礦工單位就不告訴他
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.resistOrder || !item.acceptOrderProbabilityDetermination())
                    {

                        continue;
                    }

                    //如果有其他螞蟻無所事事
                    //就叫他去掘礦
                    item.antActivity = AntActivityState.miningResource;
                    item.antMiningActivity = AntMiningActivityState.followTheMinePath;
                    item.GetComponent<SpriteRenderer>().color = Color.yellow;

                    //給他全新記錄
                    item.TranPathInfo(this);
                    //startLerpToDestination();
                }
            }
            else
            {
                for (int i = 0; i < curFloorData.ants_FollowMinePath.Count; i++)
                {
                    MinerAnt item = curFloorData.ants_FollowMinePath[i].gameObject.GetComponent<MinerAnt>();
                    //如果目標Ant不是礦工單位就不告訴他
                    if (item == null)
                    {
                        continue;
                    }
                    if (item.pathRecord.serialNumber != pathRecord.serialNumber)
                    {
                        continue;
                    }
                    if (item.antMiningActivity != AntMiningActivityState.followTheMinePath)
                    {
                        continue;
                    }
                    item.antMiningActivity = AntMiningActivityState.returnToBase;
                    item.GetComponent<SpriteRenderer>().color = Color.gray;

                    //給他全新記錄
                    item.pathRecord = new WalkingPath(pathRecord);
                    item.pathCounter = pathCounter;
                    item.Destination = item.pathRecord.path[item.pathCounter];
                    startLerpToDestination();
                }
            }

        }
    }

    public void setDestinationToMine()
    {
        Destination = targetMine.InMapV3Pos;
    }

    public override void updateObjectMapInformation()
    {
        floorData oldFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        oldFloorData.UnregisterAntData(this, isFriendly);
        if (FollowMinePathSmellRecord)
        {
            oldFloorData.UnregisterFollowMinePathAntData(this);
            FollowMinePathSmellRecord = false;
        }
        if (randWalkSmellRecord)
        {
            oldFloorData.UnregisterRandWalkAntData(this);
            randWalkSmellRecord = false;
        }

        InMapV3Pos = gameModel.instance.charWorldToMapV3(transform);

        floorData newFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        newFloorData.RegisterAntData(this, isFriendly);
        if (antMiningActivity == AntMiningActivityState.none)
        {
            newFloorData.RegisterRandWalkAntData(this);
            randWalkSmellRecord = true;
        }

        if (antMiningActivity == AntMiningActivityState.followTheMinePath)
        {
            newFloorData.RegisterFollowMinePathAntData(this);
            FollowMinePathSmellRecord = true;
        }

    }

    public override void chooseNextDestinationAndPath()
    {
        if (pathfindedInt != Destination)
        {
            //如果還有目的地就繼續走動
            startLerpToDestination();
        }
        else
        {
            //所有目的地已經到達

            //如果蟻正在掘礦，那移動方式會跟正常的不一樣
            switch (antMiningActivity)
            {
                case AntMiningActivityState.followTheMinePath:
                    goToMineByRecordPath();
                    break;
                case AntMiningActivityState.returnToBase:
                    returnBaseByRecordPath();
                    break;
            }

            if (antActivity != AntActivityState.miningResource && !inCallback)
            {
                findNewPath();
            }
            startLerpToDestination();

        }
    }

    public override void onArrivalsDestination()
    {
        recordPath();

        //更新單位資料
        updateObjectMapInformation();

        updateWorkJob();

        //leaveSomeSmell();

        collectionResourceFromMine();

        PlacingResourcesToMotherBase();

        inAttackRange = gameModel.instance.Vector2IntEquality(motherBase.instance.InMapV3Pos, InMapV3Pos);
        if (inAttackRange && inCallback)
        {
            motherBase.instance.OnSomeMinerAntEnterMotherBase(this);
            //目標一定在大本營
            return;
        }
        if (antMiningActivity == AntMiningActivityState.mining)
        {
            //如果在掘礦 先中斷
            return;
        }

        chooseNextDestinationAndPath();

        CompareOthersMiningPathAndChooseBetterOne();

        tellOtherMinerAntIfHaveMine();
    }
}
