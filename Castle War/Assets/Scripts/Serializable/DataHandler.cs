using System;
using UnityEngine;
using System.IO;

public class DataHandler
{
    public string dataPath = "";
    public string fileName = "";

    public DataHandler(string _dataPath, string _fileName)
    {
        dataPath = _dataPath;
        fileName = _fileName;
    }

    public void Save(GameData _gameData)
    {
        string directoryPath = Path.Combine(dataPath, fileName);
        string fullPath = Path.Combine(directoryPath, "saved.json");              // 完整路径，例如 .../GameData/save.json

        try
        {
            Directory.CreateDirectory(directoryPath);                    // 创建文件所在的目录
            string dataToStore = JsonUtility.ToJson(_gameData, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(dataToStore);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Fail To Write : {e}");
        }
    }


    public GameData Load()
    {
        string directoryPath = Path.Combine(dataPath, fileName);
        string fullPath = Path.Combine(directoryPath, "saved.json");
        GameData data = null;

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                data = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        else
        {
            Debug.Log("Not Found target file.");
        }
        return data;
    }
    
}
