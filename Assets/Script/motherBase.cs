using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class motherBase : building {
    public static motherBase instance;

    [SerializeField]
    List<WalkingPath> walkToMinePaths;

    public override void init() {

        base.init();

        if (!instance) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

	// Update is called once per frame
	void Update () {
	}

    public void addNewMinePath(List<Vector2Int> path) {
        walkToMinePaths.Add(new WalkingPath(path));
        print(walkToMinePaths[ walkToMinePaths.Count - 1 ].path.Count);
        OnNewMinePathFinded();
    }

    public void OnNewMinePathFinded() {
        print("new MinePath was be finded");
        print(walkToMinePaths[ walkToMinePaths.Count - 1 ].path.Count);
        List<Ant> allAntInMotherBase = gameModel.instance.getAntListInRange(InMapV3Pos,2);
        for (int i = 0; i < allAntInMotherBase.Count; i++) {
            allAntInMotherBase[ i ].pathRecord = walkToMinePaths[walkToMinePaths.Count-1].deepCopyOutputWP();

            allAntInMotherBase[ i ].antActivity = Ant.AntActivityState.miningResource;
            allAntInMotherBase[ i ].antMiningActivity = Ant.AntMiningActivityState.followTheMinePath;
        }
    }

    public void OnSomeMineIsEmpty(int mineUID) {
        for (int i = 0; i < walkToMinePaths.Count; i++) {
            WalkingPath WP = walkToMinePaths[ i ];
            if (mineUID==WP.serialNumber) {
                walkToMinePaths.Remove(WP);
            }
        }
    }

}
