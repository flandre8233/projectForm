﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorData {
    public floorSmell floorSmell;
    public List<Ant> ants;
    public List<Ant> enemyAnts;
    public mine mine;

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

    public void RegisterMineData(mine Mine) {
        mine = Mine;
    }

    public void UnregisterMineData() {
        mine = null;
    }

}



public struct Smell {
    public float smell;
    public Direction from;
}

public struct floorSmell {
    public Smell mineSmell;
    public Smell attackSmell;
    public Smell enemySmell;
    public Smell friendlySmell;
    public Smell alertSmell;
}
