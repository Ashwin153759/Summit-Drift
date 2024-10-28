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

        Debug.Log(Application.persistentDataPath);

        dataPath = Application.persistentDataPath + "/map_records.json";
        LoadRecords();
    }

    public MapRecord GetMapRecord(string mapName, int numSectors)
    {
        if (!records.ContainsKey(mapName))
        {
            records[mapName] = new MapRecord(mapName, numSectors);
        }
        return records[mapName];
    }

    public bool SaveRecord(string mapName, float lapTime, float[] sectorTimes)
    {
        // If the map doesn't exist, initialize a new MapRecord
        if (!records.ContainsKey(mapName))
        {
            int numSectors = sectorTimes.Length;
            records[mapName] = new MapRecord(mapName, numSectors);
            Debug.Log($"Created new record for map: {mapName}");
        }

        MapRecord record = records[mapName];
        bool isNewBestLap = lapTime < record.bestLapTime;
        bool isNewBestSector = false;

        // Update best lap time if the current lap is faster
        if (isNewBestLap)
        {
            record.bestLapTime = lapTime;
        }

        // Check and update each sector time if it’s a new best
        for (int i = 0; i < sectorTimes.Length; i++)
        {
            if (sectorTimes[i] < record.bestSectorTimes[i])
            {
                record.bestSectorTimes[i] = sectorTimes[i];
                isNewBestSector = true;
            }
        }

        // Save the updated records
        SaveRecords();

        // Return true if either a new best lap or sector was achieved
        return isNewBestLap || isNewBestSector;
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
