using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackerAnt : Ant
{
    public enum AttackerStyleEnem
    {
        intellect,
        fury,
        coward
    }

    [SerializeField]
    AttackerStyleEnem AttackerStyle;

    [SerializeField]
    int killCount;



    public int Damage;

    public float attackCooldown;
    public bool inAttackCd;

    Vector2Int enemyLastTimePos;

    [SerializeField]
    bool isScout;



    [SerializeField]
    Ant EnemyAnt;

    bool firstTimeFindEnemy;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        isScout = gameModel.instance.successRateCalculation(20);

        //隨機分配性格
        List<AttackerStyleEnem> attackerStyleEnems = gameModel.instance.GetEnumList<AttackerStyleEnem>();

        /*
        float avg = 100 / attackerStyleEnems.Count;
        float rand = Random.Range(0, 100);
        AttackerStyleEnem ans = AttackerStyleEnem.intellect;
        for (int i = 0; i < attackerStyleEnems.Count; i++)
        {
            AttackerStyleEnem item = attackerStyleEnems[i];

            if (rand <= (i * avg) )
            {
                ans = item;
                break;
            }
        }

        AttackerStyle = ans;
        */

        AttackerStyle = attackerStyleEnems[ Random.Range(0,attackerStyleEnems.Count-1)];

    }

    public override void ToUpdate()
    {
        base.ToUpdate();

        if (antActivity == AntActivityState.ChasingEnemy)
        {
            inAttackRange = gameModel.instance.Vector2IntEquality(EnemyAnt.InMapV3Pos, InMapV3Pos);

            //發現目標超過攻擊範圍就設定追擊路徑
            if (!inAttackRange)
            {
                Destination = EnemyAnt.InMapV3Pos;
                startLerpToDestination();
            }


            if (inAttackRange && EnemyAnt)
            {
                if (!firstTimeFindEnemy)
                {
                    //on hit
                    OnLockDownEnemy();
                }

                fightingOtherAnts(EnemyAnt);
            }
            else if (!EnemyAnt)
            {
                //如目標敵人死掉就還原工作
                if (firstTimeFindEnemy)
                {
                    //再試試找敵人
                    updateTarget();
                }
            }
        }
    }

    void updateTarget()
    {
        //未發現敵人
        if (!EnemyAnt)
        {
            //看過四周看看有沒有敵人
            if (isFriendly)
            {
                EnemyAnt = gameModel.instance.getSingleAnt_EnemyInRange(InMapV3Pos, 5);
            }
            else
            {
                EnemyAnt = gameModel.instance.getSingleAntInRange(InMapV3Pos, 5);
            }
        }

        //看過四周後如有敵人就
        if (EnemyAnt)
        {
            inAttackRange = gameModel.instance.Vector2IntEquality(EnemyAnt.InMapV3Pos, InMapV3Pos);
            //有目標敵人，但未到達攻擊範圍時
            //追擊時可中途改目標
            if (!inAttackRange)
            {
                Ant SecEnemyAnt;
                if (isFriendly)
                {
                    SecEnemyAnt = gameModel.instance.checkAnt_EnemyInThisWall(InMapV3Pos);
                }
                else
                {
                    SecEnemyAnt = gameModel.instance.checkAntInThisWall(InMapV3Pos);
                }
                if (SecEnemyAnt)
                {
                    EnemyAnt = SecEnemyAnt;
                    inAttackRange = true;
                }
            }
            onSpotTheEnemy(EnemyAnt);
        }

        if (antActivity == AntActivityState.ChasingEnemy && !EnemyAnt)
        {
            endChaseEnemy();
        }
    }

    void onSpotTheEnemy(Ant enemy)
    {
        if (!isFriendly)
        {
            antActivity = AntActivityState.ChasingEnemy;
            //已有追擊對象就將目的地設定為對象所在格
            Destination = EnemyAnt.InMapV3Pos;
            return;
        }

        switch (AttackerStyle)
        {
            case AttackerStyleEnem.intellect:
                //暫時
                antActivity = AntActivityState.callingBackups;
                enemyLastTimePos = EnemyAnt.InMapV3Pos;
                setDestinationToHeart();
                break;
            case AttackerStyleEnem.fury:
                antActivity = AntActivityState.ChasingEnemy;
                //已有追擊對象就將目的地設定為對象所在格
                Destination = EnemyAnt.InMapV3Pos;
                break;
            case AttackerStyleEnem.coward:
                findNewPath();
                break;
            default:
                break;
        }
    }

    public override void onArrivalsDestination()
    {
        //更新單位資料
        updateObjectMapInformation();

        updateTarget();

        chooseNextDestinationAndPath();
    }


    public override void updateObjectMapInformation()
    {
        //移除與註冊
        floorData oldFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        oldFloorData.UnregisterAntData(this, isFriendly);
        if (randWalkSmellRecord)
        {
            oldFloorData.UnregisterRandWalkAntData(this);
            randWalkSmellRecord = false;
        }
            oldFloorData.UnregisterAnts_AttackerData(this);
        InMapV3Pos = gameModel.instance.charWorldToMapV3(transform);

        floorData newFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        newFloorData.RegisterAntData(this, isFriendly);
        if (antActivity == AntActivityState.WalkingAround)
        {
            newFloorData.RegisterRandWalkAntData(this);
            randWalkSmellRecord = true;
        }
        oldFloorData.RegisterAnts_AttackerData(this);

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
            if (antActivity == AntActivityState.ChasingEnemy)
            {
                if (!(inAttackRange && EnemyAnt))
                {
                    //隨機選擇新坐標 並給予停頓才往下一目標前進
                    findNewPath();
                    startLerpToDestination();
                }
            }
            else if (antActivity == AntActivityState.callingBackups)
            {
                callingAlly();
                antActivity = AntActivityState.WalkingAround;
                findNewPath();
                startLerpToDestination();
            }
            else
            {
                findNewPath();
                startLerpToDestination();
            }
        }
    }

    public override void OnUnderAttackOtherAnts(int TakeDamage, Ant from)
    {
        base.OnUnderAttackOtherAnts(TakeDamage, from);
        if (inAttackRange)
        {
            EnemyAnt = from;
            antActivity = AntActivityState.ChasingEnemy;
            Destination = EnemyAnt.InMapV3Pos;
            inAttackRange = true;
        }

    }

    public void OnAttackOtherAnts(Ant target, int damage)
    {
        //AntAni.SetTrigger("isAttack");
        transform.rotation = faceAt(target.transform.position);
        target.OnUnderAttackOtherAnts(damage, this);
    }

    public void OnAttackTargetAntDead()
    {
        killCount++;
        //
        updateTarget();
        //endChaseEnemy();
    }

    void endChaseEnemy()
    {
        antActivity = AntActivityState.WalkingAround;
        EnemyAnt = null;
        inAttackRange = false;
        firstTimeFindEnemy = false;
        resetActivityToNormal();
    }

    //發現敵人鎖定目標
    public void OnLockDownEnemy()
    {
        firstTimeFindEnemy = true;
        //cutOffCurMovement();

        if (isFriendly)
        {

            //callingAlly();
        }
    }

    
    void callingAlly()
    {
        List<attackerAnt> AllyAnts = gameModel.instance.getAttackerAntListInRange(InMapV3Pos, 2) ;
        for (int i = 0; i < Random.Range(30,100); i++)
        {
                //print("FIND");
                //AllyAnts[ i ].targetEnemy = targetEnemy;
                AllyAnts[i].Destination = enemyLastTimePos;
                //AllyAnts[ i ].StopAllCoroutines();
        }

    }
    

    void OnAttackCDReady()
    {
        inAttackCd = false;
    }

    void fightingOtherAnts(Ant target)
    {
        //attacking enemy
        if (!inAttackCd)
        {
            inAttackCd = true;
            globalUpdateManager.instance.startGlobalTimer(attackCooldown, OnAttackCDReady);
            OnAttackOtherAnts(target, Damage);
        }
    }

    public override void setDestinationToRandomPoint()
    {
        if (isFriendly)
        {
            if (isScout)
            {
                Destination = gameModel.instance.getRandomPoint(gameModel.instance.mapBuildRadius, gameModel.instance.mapRadius);
            }
            else
            {
                Destination = gameModel.instance.getRandomPoint(0, gameModel.instance.mapBuildRadius);
            }
        }
        else
        {
            setDestinationToHeart();
        }
    }

}
