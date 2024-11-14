using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMainMenu : MonoBehaviour
{
    public void OnClickPlayButton()
    {
        MusicManager.instance.SetVictorious();
        GridGestion.instance.DestroyGrid();
        SceneManager.LoadScene("MainMenu");
    }
}
