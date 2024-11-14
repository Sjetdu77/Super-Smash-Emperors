using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackingGestion : MonoBehaviour
{
    public CropImage CropImage;
    public Fighter Fighter = null;
    public LandsGestion LandsGestion;
    public Animator FighterAnimator;
    public Button WinnerButton;
    public Text FighterName;

    public void SetMainFighter(Fighter Fighter)
    {
        this.Fighter = Fighter;
        CropImage.ChangeImage(Fighter);
        FighterName.text = Fighter.Name;
        if (FighterAnimator != null) FighterAnimator.SetTrigger("Show");
    }

    public void SetAllFighters(List<Fighter> Fighters) {
        LandsGestion.SetFighters(Fighters);
    }

    public void SetVictorious(Fighter Fighter)
    {
        SetMainFighter(Fighter);
        SetAllFighters(new());
        WinnerButton.gameObject.SetActive(false);
    }

    public void HideSquare() => FighterAnimator.SetTrigger("Hide");
}
