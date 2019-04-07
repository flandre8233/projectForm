using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorData {
    public floorSmell floorSmell;
    public List<Ant> ants;
    public List<Ant> ants_RandWalk;
    public List<MinerAnt> ants_FollowMinePath;
    public List<attackerAnt> ants_Attacker;

    public List<Ant> enemyAnts;
    public mine mine;

    public floorData() {
        ants = new List<Ant>();
        enemyAnts = new List<Ant>();
        ants_RandWalk = new List<Ant>();
        ants_FollowMinePath = new List<MinerAnt>();
        ants_Attacker = new List<attackerAnt>();
    }

    public void RegisterAnts_AttackerData(attackerAnt ant)
    {
        ants_Attacker.Add(ant);
    }

    public void UnregisterAnts_AttackerData(attackerAnt ant)
    {
        ants_Attacker.Remove(ant);
    }

    public void RegisterRandWalkAntData(Ant ant) {
        ants_RandWalk.Add(ant);
    }

    public void UnregisterRandWalkAntData(Ant ant) {
        ants_RandWalk.Remove(ant);
    }

    public void RegisterFollowMinePathAntData(MinerAnt ant) {
        ants_FollowMinePath.Add(ant);
    }

    public void UnregisterFollowMinePathAntData(MinerAnt ant) {
        ants_FollowMinePath.Remove(ant);
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
