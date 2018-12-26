using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorData {
    public floorSmell floorSmell;
    public List<Ant> ants;
    public List<Ant> enemyAnts;

    public floorData() {
        ants = new List<Ant>();
        enemyAnts = new List<Ant>();
    }

    public void RegisterAntData(Ant ant,bool isFriendly) {
        if (isFriendly) {
            ants.Add(ant);
        } else {
            enemyAnts.Add(ant);
        }
    }

    public void UnregisterAntData(Ant ant, bool isFriendly) {
        if (isFriendly) {
            ants.Remove(ant);
        } else {
            enemyAnts.Remove(ant);
        }
    }
}

public struct floorSmell {
    public float mineSmell;
    public float attackSmell;
    public float enemySmell;
    public float friendlySmell;
    public float alertSmell;
}
