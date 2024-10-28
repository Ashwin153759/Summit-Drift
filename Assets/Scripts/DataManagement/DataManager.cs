using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private string dataPath;
    private Dictionary<string, MapRecord> records = new Dictionary<string, MapRecord>();

    private void Awake()
    {
        if (FindObjectsOfType<DataManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        dataPath = Application.persistentDataPath + "/map_records.json";
        LoadRecords();
    }

    public MapRecord GetMapRecord(string mapName, string carName, int numSectors = 3)
    {
        string key = GetRecordKey(mapName, carName);

        if (!records.ContainsKey(key))
        {
            records[key] = new MapRecord(mapName, numSectors);
        }
        return records[key];
    }

    public bool SaveRecord(string mapName, string carName, float lapTime, float[] sectorTimes)
    {
        string key = GetRecordKey(mapName, carName);

        if (!records.ContainsKey(key))
        {
            int numSectors = sectorTimes.Length;
            records[key] = new MapRecord(mapName, numSectors);
            Debug.Log($"Created new record for map: {mapName} and car: {carName}");
        }

        MapRecord record = records[key];
        bool isNewBestLap = lapTime < record.bestLapTime;
        bool isNewBestSector = false;

        if (isNewBestLap)
        {
            record.bestLapTime = lapTime;
        }

        for (int i = 0; i < sectorTimes.Length; i++)
        {
            if (sectorTimes[i] < record.bestSectorTimes[i])
            {
                record.bestSectorTimes[i] = sectorTimes[i];
                isNewBestSector = true;
            }
        }

        SaveRecords();
        return isNewBestLap || isNewBestSector;
    }

    private string GetRecordKey(string mapName, string carName)
    {
        return $"{mapName}_{carName}";
    }

    private void LoadRecords()
    {
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            records = JsonUtility.FromJson<SerializableDictionary<string, MapRecord>>(json).ToDictionary();
        }
    }

    private void SaveRecords()
    {
        SerializableDictionary<string, MapRecord> serializableRecords = new SerializableDictionary<string, MapRecord>(records);
        string json = JsonUtility.ToJson(serializableRecords, true);
        File.WriteAllText(dataPath, json);
    }
}
