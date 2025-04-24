using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class ButtonClicks : MonoBehaviour
{
    public static ButtonClicks instance;

    public Button ContinueButton;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;

        string FilePath = $"{Application.persistentDataPath}/History.json";
        if (File.Exists(FilePath))
        {
            var History = JsonUtility.FromJson<History>(File.ReadAllText(FilePath));
            ContinueButton.interactable = History.Count > 0;
        }
        else ContinueButton.interactable = false;
    }

    public void OnClickContinueButton() => OrganizationTable.instance.OnClickContinueGrid();

    public void OnClickRandomButton() => SceneManager.LoadScene("ChooseRandom");

    public void OnClickFixedButton() => SceneManager.LoadScene("ChooseFixed");

    public void OnClickQuitButton() => Application.Quit();
}
