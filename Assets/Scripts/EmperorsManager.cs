using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmperorsManager : MonoBehaviour
{
    public NumBattleGestion NumBattleGestion;
    public AttackingGestion AttackerGestion;
    public AttackingGestion DefenderGestion;
    public Animator VictoryAnimation;
    public Button StartButton;

    Fighter[] FightersBattling;
    List<Fighter> Attackers, Defenders;
    FighterGrid Grid;
    Frontiers Frontiers;

    static public EmperorsManager instance;
    private void Awake()
    {
        if (instance != null) {
            Debug.LogWarning("EmperorsManager already exists.");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        Starting();
    }

    public void Starting()
    {
        Grid = GridGestion.instance.ActualGrid;
        Frontiers = GridGestion.instance.Frontiers;
        NumBattleGestion.ChangeNum(Grid.Battle);
        NumRemainGestion.instance.RemainChange();

        if (Grid.FightersInQuest.Count == 1)
        {
            StartButton.gameObject.SetActive(false);
            SetVictorious(Grid.FightersInQuest[0]);
        }
    }

    public void OnClickBeginButton()
    {
        var FightersConcerned = Grid.AttackSearch();

        var Random = FighterList.instance.list[0];
        Fighter OutFighter = null,
                Attacker = FightersConcerned[0],
                Defender = FightersConcerned[2];

        if (Defender == Random)
        {
            var FallenFighters = Grid.FallenFighters;
            int index = UnityEngine.Random.Range(0, FallenFighters.Count);
            OutFighter = FallenFighters[index];
            FallenFighters.Remove(OutFighter);
            List<Fighter> Reconquered = new() { OutFighter, Random },
                          OutFrontiers = Frontiers.GetFrontiers(OutFighter);

            while (Reconquered.Count < 4 && OutFrontiers.Count > 0)
            {
                var RandInt = UnityEngine.Random.Range(0, OutFrontiers.Count);
                var Land = OutFrontiers[RandInt];
                Reconquered.Add(Land);
                OutFrontiers.Remove(Land);
            }
            Grid.Conquest(OutFighter, Reconquered);

            if (Reconquered.Contains(Attacker))
            {
                NumBattleGestion.Increment();
                NumRemainGestion.instance.RemainChange();
                return;
            }
        }

        Attackers = SetCalled(FightersConcerned[1], Attacker);
        Defenders = SetCalled(FightersConcerned[3], Defender, OutFighter);

        BattleType Type;
        bool HasMainAttacker = Attackers.Contains(Attacker), HasMainDefender = Defenders.Contains(Defender);
        if (Defender == Random) Type = BattleType.Revolution;
        else if (HasMainAttacker && HasMainDefender)
        {
            if (Attackers.Count == 1 && Defenders.Count == 1) Type = BattleType.Awakening;
            else if (Attackers.Count != Defenders.Count && (Attackers.Count < 3 || Defenders.Count < 3))
                Type = BattleType.Expansion;
            else Type = BattleType.Decisive;
        }
        else if (!HasMainAttacker && !HasMainDefender)
            Type = BattleType.Conquest;
        else Type = BattleType.Siege;

        NumRemainGestion.instance.TypeSet(Type);

        if (OutFighter != null) Defender = OutFighter;

        FightersBattling = Fighter.Assemble(Attacker, Defender);
        var AttackerPossessions = Grid.GetPossessions(Attacker);
        var DefenderPossessions = Grid.GetPossessions(Defender);
        RatioManager.instance.ShowRatio(FightersBattling, AttackerPossessions.Count, DefenderPossessions.Count);

        AttackerGestion.SetMainFighter(Attacker);
        AttackerGestion.SetAllFighters(Attackers);
        DefenderGestion.SetMainFighter(Defender);
        DefenderGestion.SetAllFighters(Defenders);
        StartButton.gameObject.SetActive(false);

        foreach (var AnAttacker in Attackers) AnAttacker.GetCore().Animator.SetTrigger("Attack");
        foreach (var ADefender in Defenders) ADefender.GetCore().Animator.SetTrigger("Defend");
    }

    public void OnClickAttackVictoryButton() =>
        NextBattle(() => Grid.Conquest(FightersBattling[0], FightersBattling[1], Defenders));
    
    public void OnClickDefenseVictoryButton() =>
        NextBattle(() => Grid.Conquest(FightersBattling[1], FightersBattling[0], Attackers));

    private void SetDistance(Fighter Land, Dictionary<Fighter, int> DistanceLands, int Distance)
    {
        DistanceLands[Land] = Distance;
        List<Fighter> LandFrontiers = Frontiers.GetFrontiers(Land);

        foreach (var Frontier in LandFrontiers)
        {
            if (DistanceLands.ContainsKey(Frontier))
            {
                if (DistanceLands[Frontier] > Distance && Distance < 4) {
                    SetDistance(Frontier, DistanceLands, Distance + 1);
                }
            }
        }
    }

    private List<Fighter> SetCalled(Fighter Origin, Fighter Attacker, Fighter OutRandom = null)
    {
        if (OutRandom != null) return new(Grid.GetPossessions(OutRandom)) { OutRandom };

        List<Fighter> Lands = new() { Origin },
                      Possessions = new(Grid.GetPossessions(Attacker));

        Dictionary<Fighter, int> DistanceLands = new();
        foreach (var Possession in Possessions)
            if (!DistanceLands.ContainsKey(Possession)) DistanceLands.Add(Possession, 99);

        SetDistance(Origin, DistanceLands, 0);

        Dictionary<int, List<Fighter>> Priorities = new();
        foreach (var DistanceLand in DistanceLands)
        {
            var Land = DistanceLand.Key;
            var Priority = DistanceLand.Value;

            if (!Priorities.ContainsKey(Priority)) Priorities.Add(Priority, new());
            Priorities[Priority].Add(Land);
        }

        int i = 1;
        while (Priorities.ContainsKey(i) && Lands.Count < 4)
        {
            var IndexPriority = Priorities[i];
            if (IndexPriority.Count + Lands.Count > 4)
            {
                do
                {
                    var RandInt = UnityEngine.Random.Range(0, IndexPriority.Count);
                    var Fighter = IndexPriority[RandInt];
                    Lands.Add(Fighter);
                    IndexPriority.Remove(Fighter);
                } while (Lands.Count < 4);
            }
            else Lands.AddRange(IndexPriority);
            i++;
        }

        return Lands;
    }

    private void NextBattle(Action callback)
    {
        AttackerGestion.HideSquare();
        DefenderGestion.HideSquare();
        RatioManager.instance.HideRatio();

        foreach (var AnAttacker in Attackers) AnAttacker.GetCore().Animator.SetTrigger("Normal");
        foreach (var ADefender in Defenders)  ADefender.GetCore().Animator.SetTrigger("Normal");

        callback();

        if (Grid.FightersInQuest.Count > 1) StartButton.gameObject.SetActive(true);

        NumBattleGestion.Increment();
        NumRemainGestion.instance.RemainChange();
        Grid.Battle++;
        SaveLoadData.instance.SaveToJson();
    }

    public void SetVictorious(Fighter Fighter)
    {
        MusicManager.instance.SetVictorious(Fighter);
        AttackerGestion.SetVictorious(Fighter);
        VictoryAnimation.SetTrigger("Show");
    }
}
