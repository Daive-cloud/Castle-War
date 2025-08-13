using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveManager : SingletonManager<SaveManager>
{
    private GameData gameData;
    private List<ISaveData> savedData;
    private DataHandler dataHandler;

    void Start()
    {
        dataHandler = new DataHandler(Application.persistentDataPath,"GameData");
        savedData = FindAllSavedData();

        LoadGame();
    }

    private void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        gameData = dataHandler.Load();
        if (gameData == null)
        {
            NewGame();
        }

        foreach (var data in savedData)
        {
            data.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (var data in savedData)
        {
            data.SaveData(ref gameData);
        }
        dataHandler.Save(gameData);
    }

    private List<ISaveData> FindAllSavedData()
    {
        IEnumerable<ISaveData> savedData = FindObjectsOfType<MonoBehaviour>().OfType<ISaveData>();

        return new List<ISaveData>(savedData);
    }
}
