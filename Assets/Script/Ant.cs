using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ant : MonoBehaviour {
    enum AntActivityState {
        WalkingAround,

    }

    public int HP;
    public int Damage;

    public float attackCooldown;
    float curCD;

    public bool isFriendly;
    public bool alreadyDead;

    public Vector2Int Destination;

    public Vector2Int InMapV3Pos;

    public Vector2Int pathfindedInt;
    [SerializeField]
     Vector3 pathfindedV3;

    vector3Lerp vector3Lerp = new vector3Lerp();
    [SerializeField]
    Ant EnemyAnt;
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
    void Start () {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
        findNewPath();
        startLerpToDestination();
        runSpeed = Random.Range(0.2f,0.4f);
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
        if (isFriendly) {
            gameModel.instance.antList.Remove(this);
        } else {
            gameModel.instance.ant_enemyList.Remove(this);
        }
        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterAntData(this,isFriendly);
    }

    // Update is called once per frame
    void ToUpdate () {
        //往目的地
        toDestination();

        //updateAni;
        //AntAni.SetBool ("isMove", vector3Lerp.isLerping);

        if (inAttackRange && EnemyAnt) {
            if (!firstTimeFindEnemy) {
                //on hit
                OnLockDownEnemy();
            }

            fightingOtherAnts(EnemyAnt);
        } else {
            //如目標敵人死掉就還原工作
            if (firstTimeFindEnemy) {
                //重置鎖定敵人資料
                firstTimeFindEnemy = false;
                inAttackRange = false;
                //重新無所事事游走
                findNewPath();
                startLerpToDestination();
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
            if(curCD <= 0) {
                curCD = attackCooldown;
                OnAttackOtherAnts(target,Damage);
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
            Destination = EnemyAnt.InMapV3Pos;

            inAttackRange = gameModel.instance.Vector2IntEquality(EnemyAnt.InMapV3Pos, InMapV3Pos);
        }


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

        //查閱當前方格是否存在敵人
    }

    void attackCD() {
        if (curCD > 0) {
            curCD -= globalVarManager.deltaTime;
        }
    }

    //發現敵人鎖定目標
    public void OnLockDownEnemy() {
        firstTimeFindEnemy = true;
        StopAllCoroutines();
        //cutOffCurMovement();
        //Destination = targetEnemy.InMapV3Pos;
        //startLerpToDestination();
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

    public void OnAttackOtherAnts(Ant target,int damage) {
        //AntAni.SetTrigger("isAttack");
        transform.rotation = faceAt(target.transform.position);
        target.OnUnderAttackOtherAnts(damage,this);
    }

    public void OnAttackTargetAntDead() {
        EnemyAnt = null;
        inAttackRange = false;
    }

    public void OnUnderAttackOtherAnts(int TakeDamage,Ant from) {
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
        vector3Lerp.startLerp(transform.position, pathfindedV3 , runSpeed, null, onArrivalsDestination);
        //面向角度
    }

    float lasttimeConcentration;

    //到達下一格時
    void onArrivalsDestination() {
        //更新單位資料
        updateObjectMapInformation();

        updateTarget();

        leaveSomeSmell();
        //smellSystem();

        if (pathfindedInt != Destination) {
            //如果還有目的地就繼續走動
            startLerpToDestination();
        } else {
            //所有目的地已經到達
            findNewPath();
            StartCoroutine(enumerator());
        }

        //
    }

    void leaveSomeSmell() {
        floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        if (!isFriendly) {
            curFloorData.floorSmell.enemySmell = 50;
        } else {
            curFloorData.floorSmell.friendlySmell = 50;
        }
    }

        void smellSystem() {
        //氣味系統 試作
        floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        if (!isFriendly) {
        } else {
            if (!EnemyAnt) {
                //如果發現有敵人氣息就展開調查
                if (curFloorData.floorSmell.enemySmell > 0) {
                    Vector2Int[] nexttoPos = {
                    new Vector2Int(InMapV3Pos.x + 1, InMapV3Pos.y),
                    new Vector2Int(InMapV3Pos.x - 1, InMapV3Pos.y),
                    new Vector2Int(InMapV3Pos.x, InMapV3Pos.y + 1),
                    new Vector2Int(InMapV3Pos.x, InMapV3Pos.y - 1),
                };
                    bool keepFindSmell = false;
                    for (int i = 0; i < nexttoPos.Length; i++) {
                        if (gameModel.instance.getFloorDatas(nexttoPos[ i ]).floorSmell.enemySmell > lasttimeConcentration) {
                            //把最終目的地改成氣息所在地
                            lasttimeConcentration = gameModel.instance.getFloorDatas(InMapV3Pos).floorSmell.enemySmell;
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 0.5f);
                            keepFindSmell = true;

                            Destination = new Vector2Int(nexttoPos[ i ].x - 50, nexttoPos[ i ].y - 50);

                        }
                    }
                    if (!keepFindSmell) {
                        //lost target
                        gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
                    }
                }
            }
        }
    }

    //更新坐標
    void updateObjectMapInformation() {
        //移除與註冊
        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterAntData(this,isFriendly);

        InMapV3Pos = gameModel.instance.charWorldToMapV3(transform);

        gameModel.instance.getFloorDatas(InMapV3Pos).RegisterAntData(this,isFriendly);
    }

    //找出下個四處亂走路徑
    void findNewPath() {
        Destination = getNextMoveableDestination();
    }

    public void createPath(Vector2Int Destination) {
        //pathfindedListInt = pathfinding.StartBakeAllFloorToVector2Int(InMapV3Pos, Destination);
        pathfindedInt = pathfinding.getSinglePathData(InMapV3Pos,Destination);
        //轉為世界坐標
        pathfindedV3 = gameModel.instance.mapV3ToWorldPos(pathfindedInt);
        //將最後目的地變得有點亂數
        if (gameModel.instance.Vector2IntEquality( pathfindedInt , Destination ) ) {
            pathfindedV3.x = pathfindedV3.x + Random.Range(-0.5f, 0.5f);
            pathfindedV3.y = pathfindedV3.y + Random.Range(-0.5f, 0.5f);
        }

    }

    Vector2Int getNextMoveableDestination() {
        Vector2Int randomMapv3 = gameModel.instance.genRandomMapV3();
        if (gameModel.instance.checkThisVectorIntIsWall(randomMapv3)) {
            getNextMoveableDestination();
        }
        return randomMapv3;
    }

    IEnumerator enumerator() {

        yield return new WaitForSeconds(Random.Range(7 + gameModel.instance.delayer, 15 + gameModel.instance.delayer) );

        startLerpToDestination();
    }

}
