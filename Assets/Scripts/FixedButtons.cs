using UnityEngine;
using UnityEngine.SceneManagement;

public class FixedButtons : MonoBehaviour
{
    public static FixedButtons instance;

    private OrganizationTable tableInstance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;

        tableInstance = OrganizationTable.instance;
    }

    public void OnClickFixed64()                => tableInstance.OnClickDefaultGrid(Roster.Smash64);
    public void OnClickFixedMelee()             => tableInstance.OnClickDefaultGrid(Roster.Melee);
    public void OnClickFixedBrawl()             => tableInstance.OnClickDefaultGrid(Roster.Brawl);
    public void OnClickFixedSm4sh()             => tableInstance.OnClickDefaultGrid(Roster.Smash4);
    public void OnClickFixedSm4shDLC()          => tableInstance.OnClickDefaultGrid(Roster.Smash4DLC);
    public void OnClickFixedUltimateClassic()   => tableInstance.OnClickDefaultGrid(Roster.UltimateClassic);
    public void OnClickFixedUltimateEcho()      => tableInstance.OnClickDefaultGrid(Roster.UltimateEcho);
    public void OnClickFixedUltimateDLC()       => tableInstance.OnClickDefaultGrid(Roster.UltimateDLC);
    public void OnClickFixedUltimateAll()       => tableInstance.OnClickDefaultGrid(Roster.UltimateAll);
    public void OnClickQuitButton()             => SceneManager.LoadScene("MainMenu");
}
