using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTurn
{

}

public class BattleTurnManager
{
    public BattleTurn CurrentTurn { get; private set; } = null;

    public Queue<BattleTurn> EventsRef { get; private set; } = new Queue<BattleTurn>();

    public Queue<BattleTurn> TurnsRef { get; private set; } = new Queue<BattleTurn>();

    public Queue<BattleTurn> HistoriesRef { get; private set; } = new Queue<BattleTurn>();

    public int RoundTurn { get; private set; } = 1;

    public int RealTurn { get; private set; } = 1;

    public int GlobalTurn { get; private set; } = 1;
}

public class BattleTrigger
{

}

public class BattleTriggerManager
{
    public List<BattleTrigger> Triggers { get; private set; } = new List<BattleTrigger>();
}

public class UnitBattleActor : MonoBehaviour
{
    public string TeamCodeDebugText { get; private set; } = "Ally";

    public string GetCurrentStateName()
    {
        return "Idle";
    }
}

public class BattleProcessor : MonoBehaviour
{
    public string CurrentState { get; private set; } = "Ready";

    public int CurrentWave { get; private set; } = 0;

    public int TotalWave { get; private set; } = 3;

    public BattleTurnManager BattleTurnManager { get; private set; } = new BattleTurnManager();
}

public class BattleJudgement
{

}

public class BattleJudgementManager
{
    public List<BattleJudgement> Results { get; private set; } = new List<BattleJudgement>();
}
