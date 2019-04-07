using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ant : MonoBehaviour
{
    public Color normalStateColor;

    public GameObject inRoom;

    public enum AntActivityState
    {
        WalkingAround,
        ChasingEnemy,
        miningResource,
        callingBackups
    }

    public AntActivityState antActivity;

    public int HP;

    bool onDestination = false;
    [HideInInspector]
    public bool resistOrder;

    [HideInInspector]
    public bool inAttackRange;

    [SerializeField]
    int cost;

    public void addCost(int val)
    {
        cost += val;
    }

    public float AutonomyLevel;

    [SerializeField]
    [Range(0, 2)]
    float avgWalkTimeBetweenGridAndGrid;
    [SerializeField]
    [Range(0, 1)]
    float walkTimeRandomGap;

    public bool isFriendly;

    [HideInInspector]
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
    Animator AntAni;

    float runSpeed;

    Transform _transform = null;

    private void Awake()
    {
        //對方可能會先過start就destroy掉這class
        vector3Lerp = new AntV3Lerp(this);
        _transform = transform;
        AutonomyLevel = Random.Range(0, 100);

        //讓體形最大的單位站在最頭
        GetComponent<SpriteRenderer>().sortingOrder = (int)(10 * ((_transform.localScale.x + _transform.localScale.y + _transform.localScale.z) / 3));

        if (gameModel.instance)
        {
            updateObjectMapInformation();
        }
    }

    // Use this for initialization
    public virtual void Start()
    {
        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
        findNewPath();
        startLerpToDestination();
        runSpeed = Random.Range(avgWalkTimeBetweenGridAndGrid - walkTimeRandomGap, avgWalkTimeBetweenGridAndGrid + walkTimeRandomGap);
        normalStateColor = GetComponent<SpriteRenderer>().color;
    }

    public virtual void OnDestroy()
    {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
        if (isFriendly)
        {
            gameModel.instance.antList.Remove(this);
        }
        else
        {
            gameModel.instance.ant_enemyList.Remove(this);
        }

        gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterAntData(this, isFriendly);
        if (randWalkSmellRecord)
        {
            gameModel.instance.getFloorDatas(InMapV3Pos).UnregisterRandWalkAntData(this);
            randWalkSmellRecord = false;
        }


    }

    void OnDead()
    {
        gameController.instance.spawnMineBuilding(InMapV3Pos, getRefundNum());
        Destroy(gameObject);
    }

    public virtual int getRefundNum()
    {
        return cost / 2;
    }

    // Update is called once per frame
    public virtual void ToUpdate()
    {
        //往目的地
        toDestination();

        //如自己以死掉
        if (alreadyDead)
        {
            //destroy self
            OnDead();
        }

    }

    public void resetActivityToNormal()
    {
        //重新無所事事游走
        pathCounter = 0;
        findNewPath();
        startLerpToDestination();
    }

    public virtual void OnUnderAttackOtherAnts(int TakeDamage, Ant from)
    {
        HP -= TakeDamage;
        if (HP <= 0)
        {
            alreadyDead = true;
            attackerAnt attacker = from.GetComponent<attackerAnt>();
            if (attacker)
            {
                attacker.OnAttackTargetAntDead();
            }
            return;
        }

    }

    //取消現在所有行動
    public void cutOffCurMovement()
    {
        StopAllCoroutines();
        vector3Lerp = new AntV3Lerp(this);
    }

    //直往目的地
    void toDestination()
    {
        if (vector3Lerp.isLerping)
        {
            _transform.rotation = faceAt(pathfindedV3);
            _transform.position = vector3Lerp.update();
        }
    }

    public Quaternion faceAt(Vector3 pos)
    {
        Vector3 toTargetVector = pos - _transform.position;
        float zRotation = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0, 0, zRotation + -90));
    }

    public void startLerpToDestination()
    {
        createPath(Destination);
        vector3Lerp.startLerp(_transform.position, pathfindedV3, runSpeed);
        //面向角度
    }

    //到達下一格時
    public virtual void onArrivalsDestination()
    {
        //更新單位資料
        updateObjectMapInformation();

        chooseNextDestinationAndPath();
    }


    public virtual void chooseNextDestinationAndPath()
    {
        if (pathfindedInt != Destination)
        {
            //如果還有目的地就繼續走動
            startLerpToDestination();
        }
        else
        {
            findNewPath();
            startLerpToDestination();
        }
    }

    void leaveSomeSmell()
    {
        floorData curFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        if (!isFriendly)
        {
            curFloorData.floorSmell.enemySmell.smell = 60;
        }
        else
        {
            curFloorData.floorSmell.friendlySmell.smell = 60;
        }
    }

    public void returnBaseByRecordPath()
    {
        if (pathCounter <= 0)
        {
            setDestinationToHeart();
            return;
        }
        pathCounter--;
        Destination = pathRecord.path[pathCounter];
    }

    public void goToMineByRecordPath()
    {
        if (pathCounter >= pathRecord.path.Count - 1)
        {
            Destination = pathRecord.path[pathCounter];
            return;
        }
        pathCounter++;
        Destination = pathRecord.path[pathCounter];
    }

    public bool randWalkSmellRecord = false;

    //更新坐標
    public virtual void updateObjectMapInformation()
    {
        //移除與註冊
        floorData oldFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        oldFloorData.UnregisterAntData(this, isFriendly);
        if (randWalkSmellRecord)
        {
            oldFloorData.UnregisterRandWalkAntData(this);
            randWalkSmellRecord = false;
        }
        InMapV3Pos = gameModel.instance.charWorldToMapV3(_transform);

        floorData newFloorData = gameModel.instance.getFloorDatas(InMapV3Pos);
        newFloorData.RegisterAntData(this, isFriendly);
        if (antActivity == AntActivityState.WalkingAround)
        {
            newFloorData.RegisterRandWalkAntData(this);
            randWalkSmellRecord = true;
        }

    }

    //找出下個四處亂走路徑
    public void findNewPath()
    {
        setDestinationToRandomPoint();
    }


    public void createPath(Vector2Int Destination)
    {
        //pathfindedListInt = pathfinding.StartBakeAllFloorToVector2Int(InMapV3Pos, Destination);
        pathfindedInt = pathfinding.getSinglePathData(InMapV3Pos, Destination);
        //轉為世界坐標
        pathfindedV3 = gameModel.instance.mapV3ToWorldPos(pathfindedInt);
        //將最後目的地變得有點亂數
        if (!onDestination && gameModel.instance.Vector2IntEquality(pathfindedInt, Destination))
        {
            onDestination = true;
            pathfindedV3.x = pathfindedV3.x + Random.Range(-0.5f, 0.5f);
            pathfindedV3.y = pathfindedV3.y + Random.Range(-0.5f, 0.5f);
        }
        else if (onDestination && !gameModel.instance.Vector2IntEquality(pathfindedInt, Destination))
        {
            onDestination = false;
        }

    }

    public virtual void setDestinationToRandomPoint()
    {
        Destination = gameModel.instance.getRandomPoint();
    }

    public void setDestinationToHeart()
    {
        Destination = motherBase.instance.InMapV3Pos;
    }

    int proportionRandom(int maxVal, int equalParts)
    {
        System.Random rnd = new System.Random(System.Guid.NewGuid().GetHashCode());

        maxVal++;
        equalParts++;

        int randomVal = Random.Range(0, (maxVal * ((equalParts / 2) * (equalParts + 1))));

        int result = 0;
        for (int i = 2; i <= equalParts; i++)
        {
            float val = (maxVal * (((equalParts + 1 - i) / 2) * ((equalParts + 1 - i) + 1)));

            if (randomVal > val)
            {
                result = i - 1;
                break;
            }
        }
        rnd.Next();
        return (result * (maxVal / equalParts)) - Random.Range(0, (maxVal / equalParts));
    }

    public bool acceptOrderProbabilityDetermination()
    {
        bool output = gameModel.instance.successRateCalculation(100 - AutonomyLevel);
        if (!output)
        {
            resistOrder = true;
            globalUpdateManager.instance.startGlobalTimer(2.0f, onResistOrderEnd);
        }
        return output;
    }

    void onResistOrderEnd()
    {
        resistOrder = false;
    }

}
