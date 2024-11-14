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

    private void OnClickGrid(Mode Mode)
    {
        OrganizationTable TableInstance = OrganizationTable.instance;
        TableInstance.CreateNewGrid(Mode);
        SceneManager.LoadScene("World");
        TableInstance.gameObject.SetActive(true);
    }

    public void OnClickContinue() => OnClickGrid(Mode.Continue);

    public void OnClickStartRandomDefault() => OnClickGrid(Mode.RandomDefault);

    public void OnClickStartRandomEcho() => OnClickGrid(Mode.RandomEcho);

    public void OnClickStartFixedDefault() => OnClickGrid(Mode.FixedDefault);

    public void OnClickStartFixedEcho() => OnClickGrid(Mode.FixedEcho);

    public void OnClickStartFixedClassic() => OnClickGrid(Mode.FixedClassic);

    public void OnClickQuitButton() => Application.Quit();
}
