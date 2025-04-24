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

    static public EmperorsManager instance;
    private void Awake()
    {
        if (instance != null) {
            Debug.LogWarning("EmperorsManager already exists.");
            return;
        }
        instance = this;
    }

    private void Start() { Starting(); }

    public void Starting()
    {
        Grid = GridGestion.instance.ActualGrid;
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

        if (FightersConcerned == null)
        {
            NumBattleGestion.Increment();
            NumRemainGestion.instance.RemainChange();
            return;
        }

        var Random = FighterList.instance.list[0];
        Fighter OutFighter      = FightersConcerned["OutFighter"],
                Attacker        = FightersConcerned["Attacker"],
                Defender        = FightersConcerned["Defender"],
                AttackerLand    = FightersConcerned["AttackerLand"],
                DefenderLand    = FightersConcerned["DefenderLand"];

        Attackers = Grid.SetCalled(AttackerLand, Attacker);
        Defenders = Grid.SetCalled(DefenderLand, Defender, OutFighter);

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
        else Type = BattleType.Conquest;

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
        SaveLoadData.instance.SaveGridToJson();
    }

    public void SetVictorious(Fighter Fighter)
    {
        AttackerGestion.SetVictorious(Fighter);
        VictoryAnimation.SetTrigger("Show");
    }
}
