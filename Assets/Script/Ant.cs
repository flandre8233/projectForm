using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ant : MonoBehaviour {

    public GameObject inRoom;

    public enum AntActivityState {
        WalkingAround,
        ChasingEnemy,
        miningResource
    }



    public AntActivityState antActivity;

    public int HP;

    public int Damage;

    [SerializeField]
    int cost;

    public void addCost(int val) {
        cost += val;
    }

    public float AutonomyLevel;
    public bool resistOrder;
    float resistOrderCoolDown;

    [ SerializeField]
    [Range(0, 2)]
    float avgWalkTimeBetweenGridAndGrid;
    [SerializeField]
    [Range(0, 1)]
    float walkTimeRandomGap;

    int carryMax;

    public float attackCooldown;
    [SerializeField]
    float curCD;

    [SerializeField]
    int killCount;

    public bool isFriendly;
    public bool alreadyDead;

    public Vector2Int Destination;

    public Vector2Int InMapV3Pos;

    public WalkingPath pathRecord;
    public int pathCounter;

    public Vector2Int pathfindedInt;
    [SerializeField]
    Vector3 pathfindedV3;

    AntV3Lerp vector3Lerp = null;
    [SerializeField]
    Ant EnemyAnt;


    bool firstTimeFindEnemy;

    [SerializeField]
    Animator AntAni;

    public bool inAttackRange;



    float runSpeed;

    Transform _transform = null;

    private void Awake() {
        //對方可能會先過start就destroy掉這class
        vector3Lerp = new AntV3Lerp(this);
        _transform = transform;
        AutonomyLevel = Random.Range(0,100);

        //讓體形最大的單位站在最頭
        GetComponent<SpriteRenderer>().sortingOrder = (int)(10 * ((_transform.localScale.x + _transform.localScale.y + _transform.localScale.z) / 3));

        if (gameModel.instance) {
            updateObjectMapInformation();
        }
    }

    // Use this for initialization
    public virtual void Start() {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
        findNewPath();
        startLerpToDestination();
        runSpeed = Random.Range(avgWalkTimeBetweenGridAndGrid - walkTimeRandomGap, avgWalkTimeBetweenGridAndGrid + walkTimeRandomGap);
    }

   public virtual void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
        if (isFriendly) {
            gameModel.instance.antList.Remove(this);
        } else {
            gameModel.instance.ant_enemyList.Remove(this);
        }

        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterAntData(this, isFriendly);
        if (randWalkSmellRecord) {
            gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterRandWalkAntData(this);
            randWalkSmellRecord = false;
        }
 

    }

    void OnDead() {
        gameController.instance.spawnMineBuilding(InMapV3Pos, getRefundNum() );
        Destroy(gameObject);
    }

    public virtual int getRefundNum() {
        return cost/2;
    }

    // Update is called once per frame
    void ToUpdate() {
        //往目的地
        toDestination();

        //updateAni;
        //AntAni.SetBool ("isMove", vector3Lerp.isLerping);

        if (antActivity == AntActivityState.ChasingEnemy) {
            inAttackRange = gameModel.instance.Vector2IntEquality(EnemyAnt.InMapV3Pos, InMapV3Pos);

            //發現目標超過攻擊範圍就設定追擊路徑
            if (!inAttackRange) {
                Destination = EnemyAnt.InMapV3Pos;
                startLerpToDestination();
            }


            if (inAttackRange && EnemyAnt) {
                if (!firstTimeFindEnemy) {
                    //on hit
                    OnLockDownEnemy();
                }

                fightingOtherAnts(EnemyAnt);
            } else if (!EnemyAnt) {
                //如目標敵人死掉就還原工作
                if (firstTimeFindEnemy) {
                    //再試試找敵人
                    updateTarget();
                }
            }
        }

        //如自己以死掉
        if (alreadyDead) {
            //destroy self
            OnDead();
        }

        //CD計算器
        attackCD();

        if (resistOrderCoolDown <= 0) {
            resistOrder = false;
        } else {
            resistOrderCoolDown -= Time.deltaTime;
        }

    }

    void fightingOtherAnts(Ant target) {
        //attacking enemy
        if (curCD <= 0) {
            curCD = attackCooldown;
            OnAttackOtherAnts(target, Damage);
        }
    }

    void updateTarget() {
        //未發現敵人
        if (!EnemyAnt) {

            /*
            //先看自己這一格有沒有敵人
            if (isFriendly) {
                EnemyAnt = gameModel.instance.getSingleAnt_EnemyInRange(InMapV3Pos, 1);
            } else {
                EnemyAnt = gameModel.instance.getSingleAntInRange(InMapV3Pos, 1);
            }
            */

            if (!EnemyAnt) {
                //看過四周看看有沒有敵人
                if (isFriendly) {
                    EnemyAnt = gameModel.instance.getSingleAnt_EnemyInRange(InMapV3Pos, 3);
                } else {
                    EnemyAnt = gameModel.instance.getSingleAntInRange(InMapV3Pos, 3);
                }
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
                    //OnLockDownEnemy();
                }
            }
        }

        if (antActivity == AntActivityState.ChasingEnemy && !EnemyAnt) {
            endChaseEnemy();
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
        //cutOffCurMovement();

        if (isFriendly) {

            //callingAlly();
        }
        _transform.rotation = faceAt(EnemyAnt._transform.position);
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
        _transform.rotation = faceAt(target._transform.position);
        target.OnUnderAttackOtherAnts(damage, this);
    }

    public void OnAttackTargetAntDead() {
        killCount++;
        //
        updateTarget();
        //endChaseEnemy();
    }

    void endChaseEnemy() {
        antActivity = AntActivityState.WalkingAround;
        EnemyAnt = null;
        inAttackRange = false;
        firstTimeFindEnemy = false;
        resetActivityToNormal();
    }

    public void resetActivityToNormal() {
        //重新無所事事游走
        pathCounter = 0;
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
            antActivity = AntActivityState.ChasingEnemy;
            Destination = EnemyAnt.InMapV3Pos;
            inAttackRange = true;
        }
    }

    //取消現在所有行動
    public void cutOffCurMovement() {
        StopAllCoroutines();
        vector3Lerp = new AntV3Lerp(this);
    }

    bool onDestination;

    //直往目的地
    void toDestination() {
        if (vector3Lerp.isLerping) {
            _transform.rotation = faceAt(pathfindedV3);
            _transform.position = vector3Lerp.update();
        }
    }

    Quaternion faceAt(Vector3 pos) {
        Vector3 toTargetVector = pos - _transform.position;
        float zRotation = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0, 0, zRotation + -90));
    }

    public void startLerpToDestination() {
        createPath(Destination);
        vector3Lerp.startLerp(_transform.position, pathfindedV3, runSpeed);
        //面向角度
    }

    //到達下一格時
    public virtual void onArrivalsDestination() {
        //更新單位資料
        updateObjectMapInformation();

        updateTarget();

        chooseNextDestinationAndPath();

    }


    public virtual void chooseNextDestinationAndPath() {
        if (pathfindedInt != Destination) {
            //如果還有目的地就繼續走動
            startLerpToDestination();
        } else {
            //所有目的地已經到達
                if (antActivity == AntActivityState.ChasingEnemy) {
                    if (!(inAttackRange && EnemyAnt)) {
                        //隨機選擇新坐標 並給予停頓才往下一目標前進
                        findNewPath();
                        startLerpToDestination();
                        //現在先將"StartCoroutine"功能(停在格子休閒)停下來
                        //StartCoroutine(enumerator());
                    }
                } else {
                    findNewPath();
                    startLerpToDestination();
                }
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

    public void returnBaseByRecordPath() {
        if (pathCounter <= 0) {
            setDestinationToHeart();
            return;
        }
        pathCounter--;
        Destination = pathRecord.path[ pathCounter ];
    }

    public void goToMineByRecordPath() {
        if (pathCounter >= pathRecord.path.Count - 1) {
            return;
        }
        pathCounter++;
        Destination = pathRecord.path[ pathCounter ];
    }

    bool randWalkSmellRecord = false;

    //更新坐標
    public virtual void updateObjectMapInformation() {
        //移除與註冊
        floorData oldFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        oldFloorData.UnregisterAntData(this, isFriendly);
        if (randWalkSmellRecord) {
            oldFloorData.UnregisterRandWalkAntData(this);
            randWalkSmellRecord = false;
        }
        InMapV3Pos = gameModel.instance.charWorldToMapV3(_transform);

        floorData newFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        newFloorData.RegisterAntData(this, isFriendly);
        if (antActivity == AntActivityState.WalkingAround) {
            newFloorData.RegisterRandWalkAntData(this);
            randWalkSmellRecord = true;
        }
  
    }

    //找出下個四處亂走路徑
    public void findNewPath() {
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
        Destination = gameModel.instance.getRandomPoint();
    }



    public void setDestinationToHeart() {
        Destination = motherBase.instance.InMapV3Pos;
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



   

    public IEnumerator enumerator() {
        yield return new WaitForSeconds(0);
        //yield return new WaitForSeconds(Random.Range(7 + gameModel.instance.delayer, 15 + gameModel.instance.delayer));

    }

   public bool acceptOrderProbabilityDetermination() {
        bool output = gameModel.instance.successRateCalculation(100 - AutonomyLevel);
        if (!output) {
            resistOrder = true;
            resistOrderCoolDown = 2.0f;
        }
        return output;
    }

}
