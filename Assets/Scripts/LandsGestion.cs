using System.Collections.Generic;
using UnityEngine;

public class LandsGestion : MonoBehaviour
{
    public List<WhoFight> WhoFightList;

    public void SetFighters(List<Fighter> Fighters)
    {
        for (int i = 0; i < 4; i++) WhoFightList[i].SetImage(Fighters.Count > i ? Fighters[i] : null);
    }
}
