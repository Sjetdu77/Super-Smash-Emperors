using ProjectTools;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveLoadData : MonoBehaviour
{
    private Button ReturnButton;
    private History History;

    static public SaveLoadData instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("SaveLoadData already exists.");
            return;
        }
        instance = this;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        string FilePath = $"{Application.persistentDataPath}/History.json";
        if (File.Exists(FilePath))
            History = JsonUtility.FromJson<History>(File.ReadAllText(FilePath));
    }

    private void Update()
    {
        if (ReturnButton != null) ReturnButton.interactable = History.Count > 1;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ReturnButton != null) ReturnButton.onClick.RemoveAllListeners();

        var Object = GameObject.FindGameObjectWithTag("ReturnButton");
        if (Object != null)
        {
            ReturnButton = Object.GetComponent<Button>();
            ReturnButton.onClick.AddListener(Return);
        }
    }

    public void SaveToJson()
    {
        var ActualGrid = GridGestion.instance.ActualGrid;
        var ExportGrid = new ExportGrid(ActualGrid);
        History.Add(ActualGrid.Battle, ExportGrid);
        File.WriteAllText($"{Application.persistentDataPath}/History.json", JsonUtility.ToJson(History));
    }

    public void LoadFromJson()
    {
        string FilePath = $"{Application.persistentDataPath}/History.json";
        History = JsonUtility.FromJson<History>(File.ReadAllText(FilePath));
        var Battle = History.Count;
        GridGestion.instance.CreateGrid(new FighterGrid(History[Battle], Battle));
    }

    public void Return()
    {
        History.Remove(History.Count);
        var Key = History.Count;
        var NewGrid = History[Key];
        GridGestion.instance.CreateGrid(new FighterGrid(NewGrid, Key));
        File.WriteAllText($"{Application.persistentDataPath}/History.json", JsonUtility.ToJson(History));
        EmperorsManager.instance.Starting();
    }

    public void NewGame() 
    {
        var ExportGrid = new ExportGrid(GridGestion.instance.ActualGrid);
        History = new() { { 1, ExportGrid } };
        File.WriteAllText($"{Application.persistentDataPath}/History.json", JsonUtility.ToJson(History));
    }
}

[Serializable]
public class ExportGrid : SerializableDictionary<int, ExportYGrid>
{
    public ExportGrid(FighterGrid Grid)
    {
        foreach (var XItem in Grid)
        {
            ExportYGrid Y = new();
            foreach (var YItem in XItem.Value)
            {
                var Fighter = YItem.Value;
                Y.Add(YItem.Key, new int[] { Fighter[0].Id, Fighter[1].Id });
            }
            Add(XItem.Key, Y);
        }
    }
}

[Serializable]
public class ExportYGrid : SerializableDictionary<int, int[]> { }

[Serializable]
public class History : SerializableDictionary<int, ExportGrid> { }