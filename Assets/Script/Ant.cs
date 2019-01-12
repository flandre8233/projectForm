using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ant : MonoBehaviour {
    enum AntActivityState {
        WalkingAround,
        ChasingEnemy,
        miningResource
    }

    enum AntMiningActivityState {
        none,
        returnToBase,
        goingToMine
    }

    [SerializeField]
    AntActivityState antActivity;
    [SerializeField]
    AntMiningActivityState antMiningActivity;

    public int HP;
    public int Damage;

    public int inventory;

    int carryMax;

    public float attackCooldown;
    float curCD;

    public bool isFriendly;
    public bool alreadyDead;

    public Vector2Int Destination;

    public Vector2Int InMapV3Pos;

    public List<Vector2Int> pathRecord;
    [SerializeField]
    int pathCounter;

    public Vector2Int pathfindedInt;
    [SerializeField]
    Vector3 pathfindedV3;

    vector3Lerp vector3Lerp = new vector3Lerp();
    [SerializeField]
    Ant EnemyAnt;
    [SerializeField]
    mine targetMine;

    bool firstTimeFindEnemy;

    [SerializeField]
    Animator AntAni;

    bool inAttackRange;

    float runSpeed;

    private void Awake() {
        //對方可能會先過start就destroy掉這class

        if (gameModel.instance) {
            updateObjectMapInformation();
        }
    }

    // Use this for initialization
    void Start() {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
        findNewPath();
        startLerpToDestination();
        runSpeed = Random.Range(0.2f, 0.4f);
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
        if (isFriendly) {
            gameModel.instance.antList.Remove(this);
        } else {
            gameModel.instance.ant_enemyList.Remove(this);
        }
        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterAntData(this, isFriendly);
    }

    // Update is called once per frame
    void ToUpdate() {
        //往目的地
        toDestination();

        //updateAni;
        //AntAni.SetBool ("isMove", vector3Lerp.isLerping);

        if (antActivity == AntActivityState.ChasingEnemy) {
            if (inAttackRange && EnemyAnt) {
                if (!firstTimeFindEnemy) {
                    //on hit
                    OnLockDownEnemy();
                }

                fightingOtherAnts(EnemyAnt);
            } else {
                //如目標敵人死掉就還原工作
                if (firstTimeFindEnemy) {
                    endChaseEnemy();
                }
            }
        }
        
        //如自己以死掉
        if (alreadyDead) {
            //destroy self
            Destroy(gameObject);
        }

        //CD計算器
        attackCD();
    }

    void fightingOtherAnts(Ant target) {


        //attacking enemy
        if (!alreadyDead) {
            if (curCD <= 0) {
                curCD = attackCooldown;
                OnAttackOtherAnts(target, Damage);
            }
        }
    }

    void updateTarget() {
        //未發現敵人
        if (!EnemyAnt) {
            //看過四周看看有沒有敵人
            if (isFriendly) {
                EnemyAnt = gameModel.instance.getSingleAnt_EnemyInRange(InMapV3Pos, 3);
            } else {
                EnemyAnt = gameModel.instance.getSingleAntInRange(InMapV3Pos, 3);
            }
        }

        //看過四周後如有敵人就
        if (EnemyAnt) {
            //已有追擊對象就將目的地設定為對象所在格
            antActivity = AntActivityState.ChasingEnemy;
            Destination = EnemyAnt.InMapV3Pos;

            inAttackRange = gameModel.instance.Vector2IntEquality(EnemyAnt.InMapV3Pos, InMapV3Pos);

            //有目標敵人，但未到達攻擊範圍時
            //追擊時可中途改目標
            if (!inAttackRange) {
                Ant SecEnemyAnt;
                if (isFriendly) {
                    SecEnemyAnt = gameModel.instance.checkAnt_EnemyInThisWall(InMapV3Pos);
                } else {
                    SecEnemyAnt = gameModel.instance.checkAntInThisWall(InMapV3Pos);
                }
                if (SecEnemyAnt) {
                    EnemyAnt = SecEnemyAnt;
                    Destination = EnemyAnt.InMapV3Pos;
                    inAttackRange = true;
                }
            }
        }

        if (antActivity == AntActivityState.ChasingEnemy && !EnemyAnt) {
            endChaseEnemy();
        }
    }

    void updateWorkJob() {
        if (antActivity == AntActivityState.ChasingEnemy || antMiningActivity == AntMiningActivityState.returnToBase || !isFriendly) {
            return;
        }

        if (!targetMine) {
            targetMine = gameModel.instance.getSingleMineInRange(InMapV3Pos, 3);
        }


        if (targetMine) {
            inAttackRange = gameModel.instance.Vector2IntEquality(targetMine.InMapV3Pos, InMapV3Pos);

            antActivity = AntActivityState.miningResource;
            antMiningActivity = AntMiningActivityState.goingToMine;
            setDestinationToMine();

        }
    }
    void attackCD() {
        if (curCD > 0) {
            curCD -= globalVarManager.deltaTime;
        }
    }

    //發現敵人鎖定目標
    public void OnLockDownEnemy() {
        firstTimeFindEnemy = true;
        //StopAllCoroutines();
        cutOffCurMovement();

        if (isFriendly) {

            //callingAlly();
        }
        transform.rotation = faceAt(EnemyAnt.transform.position);
    }

    void callingAlly() {
        List<Ant> AllyAnts = gameModel.instance.getAntListInRange(InMapV3Pos, 2);
        for (int i = 0; i < AllyAnts.Count; i++) {
            if (!AllyAnts[ i ].EnemyAnt) {
                //print("FIND");
                //AllyAnts[ i ].targetEnemy = targetEnemy;
                AllyAnts[ i ].Destination = Destination;
                //AllyAnts[ i ].StopAllCoroutines();
                AllyAnts[ i ].startLerpToDestination();
            }
        }

    }

    public void OnAttackOtherAnts(Ant target, int damage) {
        //AntAni.SetTrigger("isAttack");
        transform.rotation = faceAt(target.transform.position);
        target.OnUnderAttackOtherAnts(damage, this);
    }

    public void OnAttackTargetAntDead() {
        endChaseEnemy();
    }

    void endChaseEnemy() {
        antActivity = AntActivityState.WalkingAround;
        EnemyAnt = null;
        inAttackRange = false;
        resetActivityToNormal();
    }

    void resetActivityToNormal() {
        //重新無所事事游走
        findNewPath();
        startLerpToDestination();
    }

    public void OnUnderAttackOtherAnts(int TakeDamage, Ant from) {
        HP -= TakeDamage;
        if (HP <= 0) {
            alreadyDead = true;
            from.OnAttackTargetAntDead();
            return;
        }

        if (!EnemyAnt) {
            EnemyAnt = from;
            inAttackRange = true;
        }
    }

    //取消現在所有行動
    public void cutOffCurMovement() {
        StopAllCoroutines();
        vector3Lerp = new vector3Lerp();
    }

    bool onDestination;

    //直往目的地
    void toDestination() {
        if (vector3Lerp.isLerping) {
            transform.rotation = faceAt(pathfindedV3);
            transform.position = vector3Lerp.update();
        }
    }

    Quaternion faceAt(Vector3 pos) {
        Vector3 toTargetVector = pos - transform.position;
        float zRotation = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0, 0, zRotation + -90));
    }

    public void startLerpToDestination() {
        createPath(Destination);
        vector3Lerp.startLerp(transform.position, pathfindedV3, runSpeed, null, onArrivalsDestination);
        //面向角度
    }

    float lasttimeConcentration = 60;

    //到達下一格時
    void onArrivalsDestination() {
        if (antMiningActivity == AntMiningActivityState.returnToBase) {
            returnBaseByRecordPath();
        } else {
            //記錄舊路徑
            pathRecord.Add(InMapV3Pos);
        }


        //更新單位資料
        updateObjectMapInformation();

        updateTarget();
        updateWorkJob();

        leaveSomeSmell();

        if (antMiningActivity == AntMiningActivityState.goingToMine) {
            if (inAttackRange) {
                //採集
                if (targetMine.resource >= 5) {
                    inventory += 5;
                    targetMine.resource -= inventory;


                    antMiningActivity = AntMiningActivityState.returnToBase;
                    inAttackRange = false;
                    targetMine = null;

                    //setDestinationToHeart();

                    //先設回記憶路徑最末端
                    pathCounter = pathRecord.Count - 1;
                    returnBaseByRecordPath();

                    startLerpToDestination();
                } else {
                    //根本沒有礦物
                    inAttackRange = false;
                    targetMine = null;
                    antMiningActivity = AntMiningActivityState.none;
                    antActivity = AntActivityState.WalkingAround;
                    resetActivityToNormal();
                }
            }
        }

        //暫時
        if (antMiningActivity == AntMiningActivityState.returnToBase) {
            inAttackRange = gameModel.instance.Vector2IntEquality(gameModel.instance.dungeonHeartV2Point, InMapV3Pos);
            if (inAttackRange) {
                gameModel.instance.resource += inventory;
                inventory = 0;

                //清空記憶
                pathRecord.Clear();

                inAttackRange = false;
                antMiningActivity = AntMiningActivityState.none;
                antActivity = AntActivityState.WalkingAround;
                resetActivityToNormal();

            }
        }

        if (pathfindedInt != Destination) {
            //如果還有目的地就繼續走動
            startLerpToDestination();
        } else {
            //所有目的地已經到達
            findNewPath();
            StartCoroutine(enumerator());
        }
    }

    void leaveSomeSmell() {
        floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        if (!isFriendly) {
            curFloorData.floorSmell.enemySmell.smell = 60;
        } else {
            curFloorData.floorSmell.friendlySmell.smell = 60;
        }
    }

    void returnBaseByRecordPath() {
        if (pathCounter <= 0) {
            return;
        }
        pathCounter--;
        Destination = pathRecord[ pathCounter ];
    }

    //更新坐標
    void updateObjectMapInformation() {
        //移除與註冊
        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterAntData(this, isFriendly);

        InMapV3Pos = gameModel.instance.charWorldToMapV3(transform);

        gameModel.instance.getFloorDatas(InMapV3Pos).RegisterAntData(this, isFriendly);
    }

    //找出下個四處亂走路徑
    void findNewPath() {
        setDestinationToRandomPoint();
    }

    public void createPath(Vector2Int Destination) {
        //pathfindedListInt = pathfinding.StartBakeAllFloorToVector2Int(InMapV3Pos, Destination);
        pathfindedInt = pathfinding.getSinglePathData(InMapV3Pos, Destination);
        //轉為世界坐標
        pathfindedV3 = gameModel.instance.mapV3ToWorldPos(pathfindedInt);
        //將最後目的地變得有點亂數
        if (gameModel.instance.Vector2IntEquality(pathfindedInt, Destination)) {
            pathfindedV3.x = pathfindedV3.x + Random.Range(-0.5f, 0.5f);
            pathfindedV3.y = pathfindedV3.y + Random.Range(-0.5f, 0.5f);
        }

    }

    void setDestinationToRandomPoint() {
        //Vector2Int randomMapv3 = gameModel.instance.genRandomMapV3();
        float angle = Random.Range(0, 360);
        int movableArea = gameModel.instance.mapRadius;
        Vector2Int randomMapv3 = polarCoordinates(gameModel.instance.dungeonHeartV2Point, angle, (int)(getLengthForDeg(angle) * Random.Range(0, movableArea)));
        //Vector2Int randomMapv3 = polarCoordinates(gameModel.instance.dungeonHeartV2Point, angle, (int)(getLengthForDeg(angle) * proportionRandom(movableArea, 5)) );
        Destination = randomMapv3;
        //Vector2Int randomMapv3 = polarCoordinates(new Vector2Int((1+21)/2,(-9+7)/2 ) ,Random.Range(0,360), proportionRandom(36,6) );
        /*
        if (gameModel.instance.checkThisVectorIntIsWall(randomMapv3)) {
            getNextMoveableDestination();
        }
        */
    }

    public void setDestinationToMine() {
        Destination = targetMine.InMapV3Pos;
    }

    public void setDestinationToHeart() {
        Destination = gameModel.instance.dungeonHeartV2Point;
    }

    int proportionRandom(int maxVal, int equalParts) {
        System.Random rnd = new System.Random(System.Guid.NewGuid().GetHashCode());

        maxVal++;
        equalParts++;

        int randomVal = Random.Range(0, (maxVal * ((equalParts / 2) * (equalParts + 1))));

        int result = 0;
        for (int i = 2; i <= equalParts; i++) {
            float val = (maxVal * (((equalParts + 1 - i) / 2) * ((equalParts + 1 - i) + 1)));

            if (randomVal > val) {
                result = i - 1;
                break;
            }
        }
        rnd.Next();
        return (result * (maxVal / equalParts)) - Random.Range(0, (maxVal / equalParts));
    }

    Vector2Int polarCoordinates(Vector2Int orl_point, float angle, int dist) {
        float x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
        Vector2 newPosition = orl_point;
        newPosition.x += x;
        newPosition.y += y;
        return Vector2Int.CeilToInt(newPosition);
    }
    Vector2Int polarCoordinatesButSquare(Vector2Int orl_point, float angle, int dist) {
        float radius = dist * Mathf.Cos(Mathf.PI / 4);
        float x = orl_point.x + radius * Mathf.Cos(angle);
        float y = orl_point.y + radius * Mathf.Sin(angle);
        return Vector2Int.CeilToInt(new Vector2(x, y));
    }

    float getLengthForDeg(float angle) {
        angle = ((angle + 45) % 90 - 45) / 180 * Mathf.PI;
        return 1 / Mathf.Cos(angle);
    }

    IEnumerator enumerator() {

        yield return new WaitForSeconds(Random.Range(7 + gameModel.instance.delayer, 15 + gameModel.instance.delayer));

        startLerpToDestination();
    }

}
