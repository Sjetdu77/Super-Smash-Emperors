using UnityEngine;
using UnityEngine.UI;

public class NumBattleGestion : MonoBehaviour
{
    public Text Text;
    public int NumBattle = 1;

    private void BattleChange()
    {
        Text.text = $"Battle {NumBattle}";
    }

    public void Increment()
    {
        NumBattle++;
        BattleChange();
    }

    public void ChangeNum(int num)
    {
        NumBattle = num;
        BattleChange();
    }
}
