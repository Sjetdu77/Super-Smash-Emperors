using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMainMenu : MonoBehaviour
{
    public void OnClickPlayButton()
    {
        GridGestion.instance.DestroyGrid();
        SceneManager.LoadScene("MainMenu");
    }
}
