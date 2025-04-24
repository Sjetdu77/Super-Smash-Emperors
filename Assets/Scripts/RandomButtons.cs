using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomButtons : MonoBehaviour
{
    public static RandomButtons instance;

    private OrganizationTable tableInstance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;

        tableInstance = OrganizationTable.instance;
    }

    public void OnClickRandom64()               => tableInstance.OnClickRandomGrid(Roster.Smash64);
    public void OnClickRandomMelee()            => tableInstance.OnClickRandomGrid(Roster.Melee);
    public void OnClickRandomBrawl()            => tableInstance.OnClickRandomGrid(Roster.Brawl);
    public void OnClickRandomSm4sh()            => tableInstance.OnClickRandomGrid(Roster.Smash4);
    public void OnClickRandomSm4shDLC()         => tableInstance.OnClickRandomGrid(Roster.Smash4DLC);
    public void OnClickRandomUltimateClassic()  => tableInstance.OnClickRandomGrid(Roster.UltimateClassic);
    public void OnClickRandomUltimateEcho()     => tableInstance.OnClickRandomGrid(Roster.UltimateEcho);
    public void OnClickRandomUltimateDLC()      => tableInstance.OnClickRandomGrid(Roster.UltimateDLC);
    public void OnClickRandomUltimateAll()      => tableInstance.OnClickRandomGrid(Roster.UltimateAll);
    public void OnClickQuitButton()             => SceneManager.LoadScene("MainMenu");
}
