using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RatioManager : MonoBehaviour
{
    public Animator RatioAnimator;

    public Slider AttackerSlider;
    public Image AttackerRatio;
    public Image AttackerNumber;

    public Slider DefenderSlider;
    public Image DefenderRatio;
    public Image DefenderNumber;

    static public RatioManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("RatioManager already exists.");
            return;
        }
        instance = this;
    }

    private IEnumerator FillRatio(float Ratio, float TimeEnd, Slider Slider)
    {
        var Timer = 0f;
        var StartValue = Slider.value;

        while (Timer < TimeEnd)
        {
            Slider.value = Mathf.Lerp(StartValue, (float)Ratio, Timer / TimeEnd);
            Timer += Time.deltaTime;
            yield return null;
        }

        Slider.value = Ratio;
    }

    public void ShowRatio(Fighter[] FightersBattling, int PossessionsAttacker, int PossessionsDefender)
    {
        float AttackRatio = (float)PossessionsAttacker / (PossessionsAttacker + PossessionsDefender);
        float DefendRatio = 1 - AttackRatio;

        StartCoroutine(FillRatio(AttackRatio * 1000, .5f, AttackerSlider));
        StartCoroutine(FillRatio(DefendRatio * 1000, .5f, DefenderSlider));

        RatioAnimator.SetTrigger("Show");

        var A = FightersBattling[0].Color;
        var AttackerColor = new Color(A.r, A.g, A.b);
        var D = FightersBattling[1].Color;
        var DefenderColor = new Color(D.r, D.g, D.b);

        var MoyA = (A.r + A.g + A.b) / 3;
        var MoyD = (D.r + D.g + D.b) / 3;
        var ContA = MoyA > 0.5 ? 0.1f : 0.9f;
        var ContD = MoyD > 0.5 ? 0.1f : 0.9f;

        AttackerRatio.color = AttackerColor;
        AttackerNumber.color = AttackerColor;
        DefenderRatio.color = DefenderColor;
        DefenderNumber.color = DefenderColor;

        var AttackerText = AttackerNumber.gameObject.GetComponentInChildren<Text>();
        AttackerText.text = $"{PossessionsAttacker}";
        AttackerText.color = new Color(ContA, ContA, ContA);
        var DefenderText = DefenderNumber.gameObject.GetComponentInChildren<Text>();
        DefenderText.text = $"{PossessionsDefender}";
        DefenderText.color = new Color(ContD, ContD, ContD);
    }

    public void HideRatio()
    {
        StartCoroutine(FillRatio(0, .5f, AttackerSlider));
        StartCoroutine(FillRatio(0, .5f, DefenderSlider));

        RatioAnimator.SetTrigger("Hide");
    }
}
