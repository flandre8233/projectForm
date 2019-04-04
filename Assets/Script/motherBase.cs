using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class motherBase : building {
    public static motherBase instance;

    WalkingPath newWalkPathToMineReport;

    public override void init(Vector2Int initObjectMapV2) {

        base.init(initObjectMapV2);

        if (!instance) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

	// Update is called once per frame
	void Update () {
	}

    public void addNewMinePath(WalkingPath path) {
        newWalkPathToMineReport = path;
        OnNewMinePathFinded();
    }

    [SerializeField]
    List<MinerAnt> saveMinerAnt;

    public void OnSomeMinerAntEnterMotherBase(MinerAnt minerAnt) {
        saveMinerAnt.Add(minerAnt);
        minerAnt.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void OnMinerExit() {
            StartCoroutine(MinerExitenumerator());
        StartCoroutine(MinerExitenumerator());
        StartCoroutine(MinerExitenumerator());
        StartCoroutine(MinerExitenumerator());
        StartCoroutine(MinerExitenumerator());


    }
    IEnumerator MinerExitenumerator( ) {

        while (saveMinerAnt.Count > 0) {
                yield return new WaitForSeconds(0.001f);
            if (saveMinerAnt.Count <= 0) {
                break;
            }
            MinerAnt item = saveMinerAnt[ saveMinerAnt.Count - 1 ];
            item.GetComponent<SpriteRenderer>().enabled = true;
            item.OnResetCallBack();
            saveMinerAnt.RemoveAt(saveMinerAnt.Count - 1);
        }

    }
    public void OnNewMinePathFinded() {
        List<Ant> allAntInMotherBase = gameModel.instance.getAntListInRange(InMapV3Pos,2);
        for (int i = 0; i < allAntInMotherBase.Count; i++) {
            MinerAnt item = allAntInMotherBase[ i ].GetComponent<MinerAnt>();
            if (item == null) {
                continue;
            }
            if (item.antActivity != Ant.AntActivityState.WalkingAround) {
                continue;
            }

            if (item.resistOrder || !item.acceptOrderProbabilityDetermination()) {
                continue;
            }

            item.antActivity = Ant.AntActivityState.miningResource;
            item.antMiningActivity = MinerAnt.AntMiningActivityState.followTheMinePath;

            item.pathCounter = 1;
            item.pathRecord = newWalkPathToMineReport.deepCopyOutputWP();
            item.Destination = item.pathRecord.path[ 1 ];
            item.chooseNextDestinationAndPath();

            item.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

}
