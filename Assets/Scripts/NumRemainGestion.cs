using UnityEngine;
using UnityEngine.UI;

public enum BattleType
{
    Awakening,
    Expansion,
    Siege,
    Decisive,
    Conquest,
    Revolution
}

public class NumRemainGestion : MonoBehaviour
{
    public Text Text;

    static public NumRemainGestion instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("NumRemainGestion already exists.");
            return;
        }
        instance = this;
    }

    public void RemainChange()
    {
        var Count = GridGestion.instance.ActualGrid.FightersInQuest.Count;
        if (Count > 1)
            Text.text = $"Emperors remaining: {GridGestion.instance.ActualGrid.FightersInQuest.Count}";
        else Text.text = $"An only flag!";
    }

    public void TypeSet(BattleType type)
    {
        Text.text = $"{type} Battle";
    }
}
